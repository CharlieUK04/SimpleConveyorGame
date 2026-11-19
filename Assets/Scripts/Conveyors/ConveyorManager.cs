using System;
using UnityEngine;

public class ConveyorManager : MonoBehaviour
{
    [SerializeField] private Material conveyorMaterialSet;
    [SerializeField] private float animationSpeedMultiplier = 1.5f;
    [SerializeField] private ConveyorSpeed[] availableSpeeds;
    /// <summary>
    /// Gets the available speed given an index from the array of available speeds
    /// </summary>
    /// <returns>The available speed as a float given an index from the array of available speeds</returns>
    public float GetAvailableSpeed(int index) { return availableSpeeds[index].GetSpeed(); }
    
    /// <summary>
    /// Gets the speed image sprite of a given speed index
    /// </summary>
    /// <returns>A Sprite for an icon of the given speed index</returns>
    public Sprite GetSpeedImage(int index) { return availableSpeeds[index].GetSpeedImage(); }
    
    /// <summary>
    /// Gets the length of the available speeds array
    /// </summary>
    /// <returns>An int of the length of the available speeds array</returns>
    public int GetTotalSpeeds() { return availableSpeeds.Length; }
    
    private ConveyorMateiral[] _conveyorMaterials;
    
    /// <summary>
    /// Gets the conveyor material to use based on the given speed
    /// </summary>
    /// <returns>A material matching the given speed</returns>
    public Material GetConveyorMaterial(int speed) 
    {return _conveyorMaterials[speed].BaseMaterial;}

    [Serializable]
    public class ConveyorSpeed
    {
        [SerializeField] private float speedValue = 1f;
        
        /// <summary>
        /// Gets the speed value
        /// </summary>
        /// <returns>The speed value float</returns>
        public float GetSpeed() { return speedValue; }
        [SerializeField] private Sprite speedImage;
        
        /// <summary>
        /// Gets the speed image sprite
        /// </summary>
        /// <returns>The speed image sprite</returns>
        public Sprite GetSpeedImage() { return speedImage; }
    }

    private class ConveyorMateiral
    {
        public Material BaseMaterial;
        public float CurrentOffset = 0f;
    }
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //Creates meterial instances of each material speed
        //This loads all needed materials at the start, and prevents duplicate materials.
        _conveyorMaterials = new ConveyorMateiral[GetTotalSpeeds()];
        Debug.Log(GetTotalSpeeds());
        for (int i = 0; i < _conveyorMaterials.Length; i++)
        {
            _conveyorMaterials[i] = new ConveyorMateiral();
            _conveyorMaterials[i].BaseMaterial = new Material(conveyorMaterialSet);
            _conveyorMaterials[i].BaseMaterial.SetTextureScale("_BaseMap", new Vector2(1f, 1f));
        }
        //---------------------------------------------------------
        
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _conveyorMaterials.Length; i++)
        {
            _conveyorMaterials[i].BaseMaterial.SetTextureOffset("_BaseMap", new Vector2(_conveyorMaterials[i].CurrentOffset, 0f));
            _conveyorMaterials[i].CurrentOffset += (availableSpeeds[i].GetSpeed() * animationSpeedMultiplier) * Time.deltaTime;
            //Keeps the value between 0 and 1, since anything outside this range is visibly the same as anything between 0 and 1
            //Also prevents floating point errors and overflows from infinite incrementing
            _conveyorMaterials[i].CurrentOffset = ((_conveyorMaterials[i].CurrentOffset % 1f) + 1f) % 1f;
        }
    }
}
