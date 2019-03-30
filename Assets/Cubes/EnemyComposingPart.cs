using UnityEngine;

[RequireComponent(typeof(MaterialFader))]
[RequireComponent(typeof(HP))]
public class EnemyComposingPart : MonoWithCachedTransform
{
	public MaterialFader materialFader;
	public HP hp;
}
