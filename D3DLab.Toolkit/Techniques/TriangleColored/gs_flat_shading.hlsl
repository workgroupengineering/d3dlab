﻿/**
* https://catlikecoding.com/unity/tutorials/advanced-rendering/flat-and-wireframe-shading/
**/

@geometry@

struct GSIn
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
};


[maxvertexcount(3)]
void main(
	triangle GSIn i[3],
	inout TriangleStream<GSIn> stream
)
{
    float3 p0 = i[0].position.xyz;
    float3 p1 = i[1].position.xyz;
    float3 p2 = i[2].position.xyz;

    float4 triangleNormal = float4(normalize(cross(p1 - p0, p2 - p0)), 0);
   
    i[0].normal = triangleNormal;
    i[1].normal = triangleNormal;
    i[2].normal = triangleNormal;
   
    stream.Append(i[0]);
    stream.Append(i[1]);
    stream.Append(i[2]);
}