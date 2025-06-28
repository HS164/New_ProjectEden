using UnityEngine;

public interface IPlayerSelectable
{
	// 選択先のタイプ
	public enum SelectType{
		NONE, // あるだけ
		GIMMICK, // 取得可能オブジェクト
		ENEMY // 敵
	}

	bool IsSelected();
	
	SelectType GetSelectType();

	Vector3 GetBoundsSize();
}