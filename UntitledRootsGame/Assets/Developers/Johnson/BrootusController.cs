using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrootusController : MonoBehaviour
{
    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var roots = GetComponent<RootSystem>();
            if (roots.IsRooted()) roots.Unroot();
            else roots.Root();
        }
    }
}