#ifndef UNITY_CUSTOM_SHADOW_INCLUDED
#define UNITY_CUSTOM_SHADOW_INCLUDED

//§$"$§%$ SHADER FROM THE INTERNET FOR A PROBLEM THAT TOOK WAY TO LONG TO SOLVE (http://answers.unity3d.com/questions/981011/shaders-make-mesh-partly-visible-with-shadows.html)

#include "UnityCG.cginc"
float _SelectionMinX;
float _SelectionMaxX;

float _SelectionMinY;
float _SelectionMaxY;

float _SelectionMinZ;
float _SelectionMaxZ;


uniform float4 _SelectionSphereCenter;
uniform float _SelectionSphereRadiusSquared;


struct v2f {
	V2F_SHADOW_CASTER;
	float4 posWorld : TEXCOORD1;
};

void checkIfSelected(float3 pos) {

	float3 diffPos = pos - _SelectionSphereCenter.xyz;
	float3 squaredDiff = diffPos*diffPos;

	if (!sqrt(squaredDiff.x + squaredDiff.y + squaredDiff.z) < _SelectionSphereRadiusSquared) {
		discard;
	}

	//	if (pos.x<_SelectionMinX || pos.x>_SelectionMaxX) {
	//		discard;
	//	}
}


v2f vert(appdata_base v)
{
	v2f o;
	o.posWorld = v.vertex;
	// if(o.posWorld.y < _LastVisibleY)
	// {
	TRANSFER_SHADOW_CASTER(o)
		// }
		return o;
}

float4 frag(v2f i) : COLOR
{
	checkIfSelected(i.posWorld);

SHADOW_CASTER_FRAGMENT(i)
}

#endif // UNITY_STANDARD_SHADOW_INCLUDED
