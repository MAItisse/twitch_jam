using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public float size;
    private float _time = 0f;
    private MeshRenderer _renderer;

    private void Start() {
        _renderer = GetComponent<MeshRenderer>();
    }
    
    private void Update() {
        transform.localScale = new Vector3(size*_time,size*_time,size*_time);
        Color color = _renderer.material.color;
        color.a = 1f - _time;
        _renderer.material.color = color;

        _time += Time.deltaTime;
        if (_time >= 1f) {
            Destroy(gameObject);
        }
    }
}
