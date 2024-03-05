using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRedirect : MonoBehaviour
{
    struct SlimeAgent
    {
        public Vector2 position;
        public float angle;
    }
    public ComputeShader computeShader;
    private RenderTexture resultTexture;
    private Camera cam;
    public int SlimeAgentCount;
    private ComputeBuffer slimeAgentBuffer;
    List<SlimeAgent> slimeAgents;
    void Start()
    {
        cam = GetComponent<Camera>();
        resultTexture = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24);
        resultTexture.enableRandomWrite = true;
        resultTexture.Create();

        computeShader.SetInts("imageSize", new int[] { cam.pixelWidth, cam.pixelHeight });
        computeShader.SetTexture(0, "Result", resultTexture);

        slimeAgents = new List<SlimeAgent>();
        for (int i = 0; i < SlimeAgentCount; i++)
        {
            SlimeAgent agent = new SlimeAgent();
            agent.position = new Vector2(cam.pixelWidth / 2, cam.pixelHeight / 2);
            agent.angle = UnityEngine.Random.Range(0, 2* Mathf.PI);
            slimeAgents.Add(agent);
        }
        int slimeAgentSize = sizeof(float) * 3;
        slimeAgentBuffer = new ComputeBuffer(slimeAgents.Count, slimeAgentSize);
        slimeAgentBuffer.SetData(slimeAgents);
        computeShader.SetBuffer(0, "Agents", slimeAgentBuffer);

        int agentUpdateKernel = computeShader.FindKernel("Update");
        computeShader.SetBuffer(agentUpdateKernel, "Agents", slimeAgentBuffer);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Render(source, destination);
    }
    void Render(RenderTexture source, RenderTexture destination)
    {
        computeShader.Dispatch(0, cam.pixelWidth / 8, cam.pixelHeight / 8, 1);

        int agentUpdateKernel = computeShader.FindKernel("Update");
        computeShader.SetTexture(agentUpdateKernel, "Result", resultTexture);
        computeShader.SetFloat("RandomSeed", UnityEngine.Random.Range(int.MinValue, int.MaxValue));

        computeShader.Dispatch(agentUpdateKernel, slimeAgents.Count / 20, 1, 1);
        
        Graphics.Blit(resultTexture, destination);
    }

    void OnDestroy()
    {
        slimeAgentBuffer.Release();
    }
}
