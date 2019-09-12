using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum BP3WaterMode {
    Simple = 0,
    Reflective,
    RealTimeReflective
}

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BP3Water : MonoBehaviour {
    MeshRenderer meshRenderer;
    bool insideWater;
    Dictionary<Camera, Camera> reflectionCameras = new Dictionary<Camera, Camera>();
    Dictionary<Camera, Camera> refractionCameras = new Dictionary<Camera, Camera>();
    float clipPlaneOffset = 0.07f;
    LayerMask reflectLayers = -1;
    LayerMask refractLayers = -1;
    RenderTexture reflectionTex;
    RenderTexture refractionTex;
    int texSize = 256;
    int oldReflectionTexSize;
    int oldRefractionTexSize;

    void OnEnable() {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void OnDisable() {
        if (reflectionTex) {
            DestroyImmediate(reflectionTex);
            reflectionTex = null;
        }
        if (refractionTex) {
            DestroyImmediate(refractionTex);
            refractionTex = null;
        }

        var eiter = reflectionCameras.GetEnumerator();
        while (eiter.MoveNext()) {
            DestroyImmediate(eiter.Current.Value.gameObject);
        }
        reflectionCameras.Clear();

        var riter = refractionCameras.GetEnumerator();
        while (riter.MoveNext()) {
            DestroyImmediate(riter.Current.Value.gameObject);
        }
        refractionCameras.Clear();
    }

    void OnWillRenderObject() {
        if (!enabled || !meshRenderer || !meshRenderer.enabled || !meshRenderer.sharedMaterial || meshRenderer.sharedMaterial.shader.name != "BadPerson3/Water") {
            return;
        }

        var cam = Camera.current;
        if (!cam) { return; }

        var waterMode = GetWaterMode();
        if (waterMode != BP3WaterMode.RealTimeReflective) {
            return;
        }

        if (insideWater) { return; }
        insideWater = true;


        Camera reflectionCamera, refractionCamera;
        CreateWaterObjects(cam, waterMode, out reflectionCamera, out refractionCamera);

        var pos = transform.position;
        var normal = transform.up;

        var oldPixelLightCount = QualitySettings.pixelLightCount;
        QualitySettings.pixelLightCount = 0;
        {
            UpdateCameraModes(cam, reflectionCamera);
            UpdateCameraModes(cam, refractionCamera);

            if (waterMode >= BP3WaterMode.RealTimeReflective) {
                var d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
                var reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

                var reflection = Matrix4x4.zero;
                CalculateReflectionMatrix(ref reflection, reflectionPlane);
                var oldpos = cam.transform.position;
                var newpos = reflection.MultiplyPoint(oldpos);
                reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

                var clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
                reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);
                reflectionCamera.cullingMatrix = cam.projectionMatrix * cam.worldToCameraMatrix;
                reflectionCamera.cullingMask = ~(1 << 4) & reflectLayers.value; // never render water layer
                reflectionCamera.targetTexture = reflectionTex;

                var oldCulling = GL.invertCulling;
                GL.invertCulling = !oldCulling;
                reflectionCamera.transform.position = newpos;
                var euler = cam.transform.eulerAngles;
                reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
                reflectionCamera.Render();
                reflectionCamera.transform.position = oldpos;
                GL.invertCulling = oldCulling;
                meshRenderer.sharedMaterial.SetTexture("_ReflectionTex", reflectionTex);
            }

            //if (waterMode == BP3WaterMode.Refractive) {
            //    refractionCamera.worldToCameraMatrix = cam.worldToCameraMatrix;

            //    var clipPlane = CameraSpacePlane(refractionCamera, pos, normal, -1.0f);
            //    refractionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);
            //    refractionCamera.cullingMatrix = cam.projectionMatrix * cam.worldToCameraMatrix;
            //    refractionCamera.cullingMask = ~(1 << 4) & refractLayers.value; // never render water layer
            //    refractionCamera.targetTexture = refractionTex;
            //    refractionCamera.transform.position = cam.transform.position;
            //    refractionCamera.transform.rotation = cam.transform.rotation;
            //    refractionCamera.Render();
            //    meshRenderer.sharedMaterial.SetTexture("_RefractionTex", refractionTex);
            //}
        }
        QualitySettings.pixelLightCount = oldPixelLightCount;
        insideWater = false;
    }

    public BP3WaterMode GetWaterMode() {
        var mat = meshRenderer.sharedMaterial;
        var mode = BP3WaterMode.Simple;
        if (mat.IsKeywordEnabled("BP3_WATER_REALTIME_REFLECTIVE_ON")) {
            mode = BP3WaterMode.RealTimeReflective;
        }
        else if (mat.IsKeywordEnabled("BP3_WATER_REFLECTIVE_ON")) {
            mode = BP3WaterMode.Reflective;
        }
        enabled = mode > BP3WaterMode.Simple;
        return mode;
    }

    void UpdateCameraModes(Camera src, Camera dest) {
        if (!dest) { return; }

        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;
        if (src.clearFlags == CameraClearFlags.Skybox) {
            var sky = src.GetComponent<Skybox>();
            var mysky = dest.GetComponent<Skybox>();
            if (!sky || !sky.material) {
                mysky.enabled = false;
            }
            else {
                mysky.enabled = true;
                mysky.material = sky.material;
            }
        }

        dest.farClipPlane = src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
    }

    void CreateWaterObjects(Camera currentCamera, BP3WaterMode waterMode, out Camera reflectionCamera, out Camera refractionCamera) {
        reflectionCamera = null;
        refractionCamera = null;

        if (waterMode >= BP3WaterMode.RealTimeReflective) {
            if (!reflectionTex || oldReflectionTexSize != texSize) {
                if (reflectionTex) {
                    DestroyImmediate(reflectionTex);
                }
                reflectionTex = new RenderTexture(texSize, texSize, 16);
                reflectionTex.name = "__BP3WaterReflection" + GetInstanceID();
                reflectionTex.isPowerOfTwo = true;
                reflectionTex.hideFlags = HideFlags.DontSave;
                oldReflectionTexSize = texSize;
            }

            reflectionCameras.TryGetValue(currentCamera, out reflectionCamera);
            if (!reflectionCamera) {
                var go = new GameObject(string.Format("BP3WaterReflectionCamera_{0}_For_{1}", GetInstanceID(), currentCamera.GetInstanceID()), typeof(Camera), typeof(Skybox));
                reflectionCamera = go.GetComponent<Camera>();
                reflectionCamera.enabled = false;
                reflectionCamera.transform.position = transform.position;
                reflectionCamera.transform.rotation = transform.rotation;
                reflectionCamera.gameObject.AddComponent<FlareLayer>();
                go.hideFlags = HideFlags.HideAndDontSave;
                reflectionCameras[currentCamera] = reflectionCamera;
            }
        }

        //if (waterMode == BP3WaterMode.Refractive) {
        //    if (!refractionTex || oldRefractionTexSize != texSize) {
        //        if (refractionTex) {
        //            DestroyImmediate(refractionTex);
        //        }
        //        refractionTex = new RenderTexture(texSize, texSize, 16);
        //        refractionTex.name = "__BP3WaterRefraction" + GetInstanceID();
        //        refractionTex.isPowerOfTwo = true;
        //        refractionTex.hideFlags = HideFlags.DontSave;
        //        oldRefractionTexSize = texSize;
        //    }

        //    refractionCameras.TryGetValue(currentCamera, out refractionCamera);
        //    if (!refractionCamera) {
        //        var go = new GameObject(string.Format("BP3WaterRefractionCamera_{0}_For_{1}", GetInstanceID(), currentCamera.GetInstanceID()), typeof(Camera), typeof(Skybox));
        //        refractionCamera = go.GetComponent<Camera>();
        //        refractionCamera.enabled = false;
        //        refractionCamera.transform.position = transform.position;
        //        refractionCamera.transform.rotation = transform.rotation;
        //        refractionCamera.gameObject.AddComponent<FlareLayer>();
        //        //go.hideFlags = HideFlags.HideAndDontSave;
        //        refractionCameras[currentCamera] = refractionCamera;
        //    }
        //}
    }

    Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign) {
        Vector3 offsetPos = pos + normal * clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane) {
        reflectionMat.m00 = (1.0f - 2.0f * plane[0] * plane[0]);
        reflectionMat.m01 = (-2.0f * plane[0] * plane[1]);
        reflectionMat.m02 = (-2.0f * plane[0] * plane[2]);
        reflectionMat.m03 = (-2.0f * plane[3] * plane[0]);

        reflectionMat.m10 = (-2.0f * plane[1] * plane[0]);
        reflectionMat.m11 = (1.0f - 2.0f * plane[1] * plane[1]);
        reflectionMat.m12 = (-2.0f * plane[1] * plane[2]);
        reflectionMat.m13 = (-2.0f * plane[3] * plane[1]);

        reflectionMat.m20 = (-2.0f * plane[2] * plane[0]);
        reflectionMat.m21 = (-2.0f * plane[2] * plane[1]);
        reflectionMat.m22 = (1.0f - 2.0f * plane[2] * plane[2]);
        reflectionMat.m23 = (-2.0f * plane[3] * plane[2]);

        reflectionMat.m30 = 0.0f;
        reflectionMat.m31 = 0.0f;
        reflectionMat.m32 = 0.0f;
        reflectionMat.m33 = 1.0f;
    }
}