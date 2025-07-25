using UnityEngine;
using System;
using System.Collections.Generic;

public class DelegateList<T> where T : Delegate {

    private List<T> delegates;

    public void Register(T d){
        if(delegates == null){
            delegates = new List<T>();
        }

        delegates.Add(d);
    }

    public void Invoke(params System.Object[] args){
        if(delegates != null){
            foreach(T d in delegates){
                d.DynamicInvoke(args);
            }
        }
    }
};
