using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverrideFunctionTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var test = GetProgression<ulong>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Function of type <T>
    public T GetProgression<T>()
    {
        return default(T);
    }
    
    public void GetProgression()
    {
        
    }
}
