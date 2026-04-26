using UnityEngine;

public class TimeOfDayMaterialChanger : MonoBehaviour 
{
	public RendererType rendererType;

	public enum RendererType { Mesh, Particle, Sprite }
    WorldManager worldManager;
    MeshRenderer meshRenderer;
    SpriteRenderer spriteRenderer;
    ParticleSystemRenderer particleSystemRenderer;
 
	void Start() 
	{
		worldManager = WorldManager.Instance;
		worldManager.OnTimeOfDayChanged += UpdateMaterialColor;
	}

	public void UpdateMaterialColor()
	{
		if(rendererType == RendererType.Mesh)
		{
			meshRenderer = GetComponent<MeshRenderer>();
			meshRenderer.material.color = new Color(worldManager.mainMaterial.color.r, worldManager.mainMaterial.color.g, worldManager.mainMaterial.color.b, meshRenderer.material.color.a);
			return;
		}

		if(rendererType == RendererType.Sprite)
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			spriteRenderer.color = new Color(worldManager.mainMaterial.color.r, worldManager.mainMaterial.color.g, worldManager.mainMaterial.color.b, spriteRenderer.color.a);
			return;
		}

		if(rendererType == RendererType.Particle)
		{
			particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
			particleSystemRenderer.material.color = new Color(worldManager.mainMaterial.color.r, worldManager.mainMaterial.color.g, worldManager.mainMaterial.color.b, particleSystemRenderer.material.color.a);
			return;
		}
	}
}