using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectComponent : ComponentCache
{
    public List<ParticleSystem> particles;
    public float psDurationTime;
    public float aniDurationTime;
    public float Duration { get { return Math.Max(psDurationTime, aniDurationTime); } }


    public override bool UpdateComponent()
    {
        if (particles == null) particles = new List<ParticleSystem>();
        else particles.Clear();


        if(!base.UpdateComponent())return false ;

        ResetMaxLifeTimeByChlidren(this.transform);
        CalculatePsDura();
        CalculateAniDura();

        return true;
    }

    protected override void SearchComponent(Transform trans)
    {
        ParticleSystem particleSystem = trans.GetComponent<ParticleSystem>();
        if (particleSystem != null) particles.Add(particleSystem);

    }
   /* public void INgi()
    {
        //GLog.Log("animators count = " + animators.Length);
        //GLog.Log("particles count = " + particles.Length);
        ResetMaxLifeTimeByChlidren(this.transform);
        CalculatePsDura();
        CalculateAniDura();
    }*/


    void CalculateAniDura()
    {
        for (int i = 0; i < animators.Count; i++)
        {
            if (animators[i] != null)
            {
                AnimatorStateInfo animatorStateInfo = animators[i].GetCurrentAnimatorStateInfo(0);
                if (!animatorStateInfo.loop)
                    if (animatorStateInfo.length > aniDurationTime)
                        aniDurationTime = animatorStateInfo.length;
            }
        }
        aniDurationTime += 0.3f;
    }

    void CalculatePsDura()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            if (particles[i] != null)
            {
                float time = particles[i].main.startLifetime.constantMax + particles[i].main.startDelay.constantMax;
                if (time > psDurationTime)
                    psDurationTime = time;
            }
        }
        psDurationTime += 0.3f;
    }

    float ResetMaxLifeTimeByChlidren(Transform go)
    {
        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        if (go.transform.childCount == 0)
            return ps == null ? 0f : ps.main.startLifetime.constantMax;
        float maxChildLifeTime = 0;

        for (int i = 0; i < go.transform.childCount; i++)
        {
            float childLifeTime = ResetMaxLifeTimeByChlidren(go.transform.GetChild(i));
            if (childLifeTime > maxChildLifeTime)
                maxChildLifeTime = childLifeTime;
        }
        if (ps != null)
        {
            if (ps.main.startLifetime.constantMax > maxChildLifeTime)
            {
                var m = ps.main;
                m.startLifetime = ps.main.startLifetime.mode == ParticleSystemCurveMode.Constant ? new ParticleSystem.MinMaxCurve(maxChildLifeTime) :
                     new ParticleSystem.MinMaxCurve(ps.main.startLifetime.constantMin, maxChildLifeTime);
                //   GLog.Log("Change ParticleSystem startLifetime~!~!");
            }
        }
        return maxChildLifeTime;
    }

    public void Play()
    {
        if (animators != null)
        {
            for (int i = 0; i < animators.Count; i++)
            {
                if (animators[i] != null && animators[i].gameObject.activeSelf)
                {
                    AnimatorStateInfo animatorStateInfo = animators[i].GetCurrentAnimatorStateInfo(0);
                    animators[i].Play(animatorStateInfo.fullPathHash, 0, 0);
                }
            }
        }
        if(particles != null)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i] != null)
                {
                    particles[i].Play();
                    var m = particles[i].main;
                }
            }
        }
    }
    public void SetSpeed(float speedScale)
    {
        for (int i = 0; i < animators.Count; i++)
        {
            if (animators[i] != null)
            {
                animators[i].speed = speedScale;
            }
        }
        for (int i = 0; i < particles.Count; i++)
        {
            if (particles[i] != null)
            {
                var m = particles[i].main;
                m.simulationSpeed = speedScale;
            }
        }
    }
}
