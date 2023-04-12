using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OriginPointTest : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        //Trying to find middle origin of the non center pivoted objects
        Renderer cache = this.transform.GetComponent<Renderer>();
        Gizmos.DrawCube(cache.bounds.center,new Vector3(5,5,5));
    }
}
