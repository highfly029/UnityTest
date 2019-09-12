using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 资源prefab的component序列化缓存对象；
/// 可扩展
/// </summary>
public class ComponentCache : MonoBehaviour
{
    public Animator animator = null;

    public List<Renderer> renderers = null;
    public List<SkinnedMeshRenderer> skinMeshRenders = null;
    public List<Animator> animators = null;

    [ContextMenu("Update Component")]
    public void MaskUpDate()
    {
        UpdateComponent();
    }
   
    public virtual bool UpdateComponent()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = gameObject.AddComponent<Animator>();
        if (renderers == null) renderers = new List<Renderer>();
        if (animators == null) animators = new List<Animator>();
        if (skinMeshRenders == null) skinMeshRenders = new List<SkinnedMeshRenderer>();

        renderers.Clear();
        animators.Clear();
        skinMeshRenders.Clear();

        RecursionFindCompoent(transform);

        return true;
    }

    private void RecursionFindCompoent(Transform trans)
    {
        SearchComponent(trans);
        for (int i = 0; i < trans.childCount; i++)
        {
            RecursionFindCompoent(trans.GetChild(i));
        }
    }

    protected virtual void SearchComponent(Transform trans)
    {
        Animator animator = trans.GetComponent<Animator>();
        if (animator != null) animators.Add(animator);

        Renderer renderer = trans.GetComponent<Renderer>();
        if (renderer != null) renderers.Add(renderer);
        
        if (renderer != null&& renderer is SkinnedMeshRenderer) skinMeshRenders.Add((SkinnedMeshRenderer)renderer);
    }

    protected virtual void OnDestroy()
    {

    }
}
