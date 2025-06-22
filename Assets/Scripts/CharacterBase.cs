using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// キャラクター、敵とプレイヤー共通の親
/// </summary>
public class CharacterBase : MonoBehaviour
{
    /*---------------------------------------
     * いったん本番でやりたい、プレイヤーと敵は同じ親クラスを継承する用のキャラクラスは作った
     * めんどいからこれは正直使わないかなプロトタイプで
     * 消してほしいなら消しとく
     * 本番ではさらに親の EntityBase とかでダメージ受け取れるオブジェクトとかも含む奴もいいかも
     * 敵、プレイヤー、オブジェクトにもあたるものがあった場合に便利かも
     * --------------------------------------*/
}
