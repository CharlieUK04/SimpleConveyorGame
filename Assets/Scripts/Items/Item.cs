using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    [HideInInspector] public ConveyorBelt currentConveyorBelt = null;
    [HideInInspector] public float progressAcrossBelt = 0f;
    [HideInInspector] public bool isFalling = false;
    [HideInInspector] public float itemYOffset = 0f;
    private Vector3 _velocity = Vector3.zero;

    private void Awake()
    {
        itemYOffset = transform.position.y - GetComponent<Collider>().bounds.min.y;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isFalling = true;
        _velocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentConveyorBelt != null) return;

        _velocity += Physics.gravity * Time.deltaTime;
        transform.position += _velocity * Time.deltaTime;

        if (transform.position.y < -10f)
            Destroy(gameObject);
    }
}
