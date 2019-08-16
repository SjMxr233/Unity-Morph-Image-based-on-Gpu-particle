Shader "shader test/particle"
{
	Properties
	{
		_Color("Color",color)=(1,1,1,1)
		_MainTex ("_MainTex", 2D) = "white" {}
		_MainTex2("_MainTex2",2D)="white"{}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		Pass
		{
			CGPROGRAM
			#pragma target 4.5
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct Particle
			{
				float3 position;
				float3 CustomPos;
				float2 uv;
			};			
			StructuredBuffer<Particle> _ParticleBuffer;
			sampler2D _MainTex,_MainTex2;
			fixed4 _Color;
			float _lerp,_Size;
			float4x4 _GameobjectMatrix;
			struct appdata{
				float4 vertex:POSITION;
			};
			struct v2f{
				float4 pos:SV_POSITION;
				float2 uv:TEXCOORD0;
			};
			float4x4 GetModelToWorldMatrix(float3 pos)
			{
				float4x4 transformMatrix=float4x4(
						_Size,0,0,pos.x,
						0,_Size,0,pos.y,
						0,0,_Size,pos.z,
						0,0,0,1
				);
				return transformMatrix;
			}
			v2f vert(appdata v,uint instanceID :SV_INSTANCEID)
			{
				v2f o;
				Particle particle = _ParticleBuffer[instanceID]; 
				float4x4 WorldMatrix=GetModelToWorldMatrix(particle.position.xyz);
				WorldMatrix=mul(_GameobjectMatrix,WorldMatrix);
				v.vertex = mul(WorldMatrix, v.vertex);
				o.pos=mul(UNITY_MATRIX_VP,v.vertex);
				o.uv = _ParticleBuffer[instanceID].uv;
				return o;
			}
			fixed4 frag(v2f i):SV_Target
			{
				fixed4 col=tex2D(_MainTex,i.uv);
				fixed4 col2=tex2D(_MainTex2,i.uv);
				col=lerp(col,col2,_lerp)*_Color;
				return col;
			}
			ENDCG
		}
		
	}
	FallBack Off
}