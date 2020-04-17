using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    ParticleSystem cloudSystem;
    public Color colour;
    public Color lining;
    bool painted = false;
    public int numberOfParticles;
    public float minSpeed;
    public float maxSpeed;
    public float distance;
    Vector3 startPosition;
    float speed;
    
    
    // Start is called before the first frame update
    void Start()
    {
        cloudSystem = this.GetComponent<ParticleSystem>();
        Spawn();
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(0, 0, 0.01f);

        if(Vector3.Distance(this.transform.position,startPosition) > distance) {
            Spawn();
        }
    }

    /// <summary>
    /// Sets the location of where the cloud should be. Calculates it speed and origin point
    /// </summary>
    void Spawn() {

        //extend the range of the scale on either side of the manager center
        float xpos = UnityEngine.Random.Range(-0.5f, 0.5f);
        float ypos = UnityEngine.Random.Range(-0.5f, 0.5f);
        float zpos = UnityEngine.Random.Range(-0.5f, 0.5f);
        this.transform.localPosition = new Vector3(xpos, ypos, zpos);
        speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        startPosition = this.transform.position;
    }
}
