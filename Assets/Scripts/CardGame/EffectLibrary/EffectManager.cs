﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;
//using CardEffects;


public class Effect
{
    public delegate bool Fun(ICardPlayerState player, int a);
    public Fun fun;
}


public interface IEffectManager
{
    bool CheckCanPlay(Card card);
    bool Hold(Card card);
    bool Play(Card card);
}

public class EffectManager : IEffectManager
{
    private static EffectManager _instance = null;
    public static EffectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EffectManager();
            }
            return _instance;
        }
    }

    private Dictionary<string, Effect> effects = new Dictionary<string, Effect>();

    private Dictionary<string, Effect> InitDict()
    {
        Dictionary<string, Effect> res = new Dictionary<string, Effect>();
        var t = typeof(EffectsLibrary);
        foreach (var m in t.GetMethods())
        {
            Effect eff = new Effect();
            eff.fun = (Effect.Fun)Delegate.CreateDelegate(typeof(Effect.Fun), EffectsLibrary.Instance, m);
            effects.Add(m.Name, eff);
        }
        return res;
    }

    public bool CheckCanPlay(Card card)
    {
        return Examine(card.condition, card.condition_scale);
    }

    public bool Hold(Card card)
    {
        return Examine(card.hold_effect, card.hold_effect_scale);
    }

    public bool Play(Card card)
    {
        Debug.Log("使用卡牌： " + card.name);
        bool res = true;
        if (CheckCanPlay(card))
        {
            res = Examine(card.effect, card.effect_scale);
            Examine(card.post_effect, card.post_effect_scale);
            return res;
        }
        return false;
    }


    private bool Examine(List<string> effects, List<int> scale)
    {
        bool res = true;
        for (int i = 0; i < effects.Count; i++)
        {
            res &= Execute(effects[i], scale[i]);
        }
        return res;
    }


    private bool Execute(string key, int scale = 0)
    {
        if (!effects.TryGetValue(key, out Effect eff))
        {
            var t = typeof(EffectsLibrary);
            var method = t.GetMethod(key);
            if (method == null)
            {
                Debug.LogError(string.Format("对应的Effect{0}不存在，请检查配置！", key));
                return true;
            }
            eff = new Effect();
            eff.fun = (Effect.Fun)Delegate.CreateDelegate(typeof(Effect.Fun), EffectsLibrary.Instance, method);
            effects.Add(key, eff);
        }
        return eff.fun(CardGameManager.Instance.MainPlayerState,scale);
    }
}