using System;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Playables;
using UnityEngine.Animations;
//Colliderのデストロイタイミング
/// <summary>
/// 駒が目標地点まで移動していくまでの処理を実装するクラス
/// </summary>
public class TurnDeside : ColorPallet
{
    InGameManager _inGameManager;
    OpenSelectableArea _openSelectableArea;
    AddPieceFunction _addPieceFunction;
    CollisionEvent _collisionEvent; //いらない
    SpriteRenderer _selectedTileSpriteRenderer; //移動後の透明化用
    GameObject _selectedPieceObj;
    Piece _selectedPiece;
    Squere _selectedSquere;
    Squere _targetSquere;
    Squere _enpassantSquere;
    Animator _selectedPieceAnimatorController;
    Animator _targetPieceAnimatorController;
    RuntimeAnimatorController _selectedPieceRuntimeAnimator;
    AnimationCurve _endPositionCurve;
    GameObject _collider2DPrefab;
    GameObject _RAttackEffectObj;
    GameObject _BAttackEffectObj;
    GameObject _QAttckEffectObj;
    GameObject _targetObj;
    GameObject _enpassantObj;
    // GameObject _hitStopObj;
    public GameObject _promotionObj; //試験的
    PlayableGraph _playableGraph;
    AnimationPlayableOutput _animationPlayableOutput;
    bool _isDirectionRight;
    float _direction;
    float _Direction { get {return _direction ;} set { _direction = value; if (_direction < 0){ _isDirectionRight = false; } else {_isDirectionRight = true;}}}//リファクタ
    
    private void Start()
    {
        _inGameManager = GetComponent<InGameManager>();
        _openSelectableArea = GetComponent<OpenSelectableArea>();
        _collider2DPrefab = _inGameManager._Collider2DPrefab;
        _collisionEvent = _collider2DPrefab.GetComponent<CollisionEvent>();
        _addPieceFunction = GetComponent<AddPieceFunction>();
        _RAttackEffectObj = transform.GetChild(0).gameObject;
        _BAttackEffectObj = transform.GetChild(1).gameObject;
        // _hitStopObj = transform.GetChild(2?).gameObject;
    }
    /// <summary>
    /// _SelectedTileSprite上でマウスクリックされた時に一度だけ呼び出される
    /// </summary>
    /// <param name="currentSpriteRenderer"></param>
    /// <param name="selectedPieceObj"></param>
    /// <param name="selectedPiece"></param>
    /// <param name="selectedSquere"></param>
    /// <param name="targetSquere"></param>
    public void StartTurnDeside(SpriteRenderer currentSpriteRenderer, GameObject selectedPieceObj, Piece selectedPiece, Squere selectedSquere, Squere targetSquere)
    {
        //引数をキャッシュ化
        _selectedTileSpriteRenderer = currentSpriteRenderer;
        _selectedPieceObj = selectedPieceObj;
        _selectedPiece = selectedPiece;
        _selectedSquere = selectedSquere;
        _targetSquere = targetSquere;
        _selectedPieceAnimatorController = _selectedPieceObj.GetComponent<Animator>();
        _selectedPieceRuntimeAnimator = _selectedPieceAnimatorController.runtimeAnimatorController;
        //移動に伴って_SelectedPieceObjやSquererなどをアップデート → ラムダ候補
        char[] updateName = _selectedPieceObj.name.ToCharArray();
        updateName[2] = (char)('0' + _targetSquere._SquereTilePos.y);
        updateName[4] = (char)('0' + _targetSquere._SquereTilePos.x);
        _selectedPieceObj.name = new string(updateName);
        _selectedSquere._IsOnPieceObj = null;
        _Direction = _targetSquere._SquereTilePos.x - _selectedSquere._SquereTilePos.x;
        //初めて移動した駒であればrotation.zは 0 という勝手な仕様
        if (_selectedPieceObj.transform.rotation.z == 0)
        {
            //OpenSelectableAreaで利用する
            _selectedPieceObj.transform.rotation = Quaternion.Euler(0, 0, 360);
            //ルーク・キングが動いた瞬間に、一部のキャスリングが二度と使用できなくなる
            if (updateName[0] == 'R' || updateName[0] == 'K') //stringにしたい
            {
                SquereID id = _selectedSquere._SquereID;
                switch (id)
                {
                    case SquereID.a1:
                        _inGameManager.IsWhiteShortCastling = () => false;
                        break;
                    case SquereID.h1:
                        _inGameManager.IsWhiteLongCastling = () => false;
                        break;
                    case SquereID.a8:
                        _inGameManager.IsBlackShortCastling = () => false;
                        break;
                    case SquereID.h8:
                        _inGameManager.IsBlackLongCastling = () => false;
                        break;
                    case SquereID.d1:
                        _inGameManager.IsWhiteShortCastling = () => false;
                        _inGameManager.IsWhiteLongCastling = () => false;
                        break;
                    case  SquereID.d8:
                        _inGameManager.IsBlackShortCastling = () => false;
                        _inGameManager.IsBlackLongCastling = () => false;
                        break;
                }
            }
        }
        //Collider生成のif文
        if (_targetSquere._IsOnPieceObj) //enpassantの判断後にこれも判断すれば良い
        {
            //移動先に敵駒がある場合の処理
            CollisionEvent.CollisionAction = RegisterTarget;
            Instantiate(_collider2DPrefab, _targetSquere._SquerePiecePosition, Quaternion.identity);
        }
        else if (_targetSquere._IsActiveEnpassant)
        {
            CollisionEvent.CollisionAction = RegisterEnpassantTarget; //親オブジェクトを取得するためのメソッドを登録する;
            Instantiate(_collider2DPrefab, _targetSquere._SquerePiecePosition, Quaternion.identity);
        }
        StartRunAnimation();
        //攻撃 → 移動 → Idle
        //移動 → 攻撃 → Idle の２パターンにこの後枝分かれをする
    }
    /// <summary>
    /// CollisionEvent.csからの衝突情報で移動先にあるtargetObjを取得する
    /// </summary>
    void RegisterTarget(GameObject collisionObj)
    {
        _targetObj = collisionObj;
        _targetPieceAnimatorController = _targetObj.GetComponent<Animator>();
    }
    /// <summary>
    /// CollisionEvent.csからの衝突情報で移動先にあるオブジェクトから親に指定されているtargetObjを取得する
    /// </summary>
    /// <param name="collisionObj"></param>
    void RegisterEnpassantTarget(GameObject collisionObj)
    {
        if (collisionObj.name.First() == 'U')
        {
            _targetObj = collisionObj.transform.parent.gameObject;
            _targetPieceAnimatorController = _targetObj.GetComponent<Animator>();
            string[] search = _targetObj.name.Split("_");
            Squere onEnemySquere = _inGameManager._SquereArrays[int.Parse(search[1])][int.Parse(search[2])];
            //unpassantにより倒される敵の下にあるSuereの_IsOnPieceを更新する
            onEnemySquere._IsOnPieceObj = null;
        }
    }
    /// <summary>
    /// "Run"アニメーションを作成し、PlayableGraphで再生する。動作が独立している。
    /// </summary>
    public void StartRunAnimation()
    {
        //knightの時は攻撃の移動に合わせて始点と終点を指定したい
        AnimationCurve animationCurveX = AnimationCurve.Linear(0f, _selectedPieceObj.transform.position.x, 1f, _targetSquere._SquerePiecePosition.x);
        AnimationCurve animationCurveY = AnimationCurve.Linear(0f, _selectedPieceObj.transform.position.y, 1f, _targetSquere._SquerePiecePosition.y);
        //"Run"という名前のついたanimationClipからコピーを新規作成
        AnimationClip animationClip = _selectedPieceRuntimeAnimator.animationClips.FirstOrDefault(clip => clip.name.Contains("Run")); 
        //新しく作成・編集したAnimationCurveをAnimationClipに代入する
        animationClip.SetCurve("", typeof(Transform), "localPosition.x", animationCurveX);
        animationClip.SetCurve("", typeof(Transform), "localPosition.y", animationCurveY);
        //PlayableGraphを作成
        _playableGraph = PlayableGraph.Create();
        //AnimationClipPlayableを作成
        AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(_playableGraph, animationClip);
        //AnimationPlayableOutputを作成してAnimatorと連結
        _animationPlayableOutput = AnimationPlayableOutput.Create(_playableGraph, "AnimOutput", _selectedPieceAnimatorController);
        _animationPlayableOutput.SetSourcePlayable(animationClipPlayable);
        //再生
        _playableGraph.Play();
    }
    /// <summary>
    /// MoveAnimationの再生後にAnimationEventで１回呼ばれる。動作が独立している。
    /// </summary>
    public void StartAttackAnimation() //移動 → 攻撃 の駒はその後も少し移動する必要がある
    {
        //enpassantであれば
        if (_selectedPiece._PieceName == "P"
            &&
            _targetSquere._IsActiveEnpassant)
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Stop();
                _playableGraph.Destroy();
            }
            _selectedPieceObj.GetComponent<SpriteRenderer>().flipX = !_selectedPieceObj.GetComponent<SpriteRenderer>().flipX; //攻撃するPieceの向いている方向を反転する
            _selectedPieceAnimatorController.Play("P_Attack");
            _targetSquere._IsActiveEnpassant = false;
        }
        else if (_targetSquere._IsOnPieceObj)
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Stop();
                _playableGraph.Destroy();
            }
            string search = $"{_selectedPiece._PieceName}_Attack";
            _selectedPieceAnimatorController.Play(search);
            _targetSquere._IsOnPieceObj = null;
        }
    }
    /// <summary>
    /// SelectedPieceのIdleAnimationを再生する。動作が独立している。TurnDesid.csが処理をしなくなる直前に必ず１回呼び出される。
    /// </summary>
    public void StartIdleAnimation()
    {
        if (_playableGraph.IsValid())
        {
            _playableGraph.Stop();
            _playableGraph.Destroy();
        }
        _selectedPieceObj.GetComponent<SpriteRenderer>().flipX = !_inGameManager._IsWhite;
        string search = new string($"{_selectedPiece._PieceName}_Idle");
        _selectedPieceAnimatorController.Play(search);
        //ターンを終えた後の処理
        EndTurn();
        // _miniBoard.UpdateMiniBorad();
        //TurnDeside側から通知できること
        //_selectedPieceがどこに動いたのか
        //_誰が倒されたのか（重要）
        //Promotionが起きたかどうか（重要）
        //MiniBoradに通知することで両者の攻撃範囲が自動で取得できる、という構造を創る
    }
    /// <summary>
    /// 敵のTakeHitAnimationを再生する。動作が独立している。
    /// </summary>
    public void StartTakeHitAnimation()
    {
        string search = new string($"{_targetObj.name.First()}_TakeHit");
        _targetPieceAnimatorController.Play(search);
    }
    /// <summary>
    /// 敵のDeathAnimationを再生する。動作が独立している。
    /// </summary>
    public void StartDeathAnimation()
    {
        // _hitStopObj.SetActive(true);
        _targetObj.GetComponent<SpriteRenderer>().flipX = !_selectedPieceObj.GetComponent<SpriteRenderer>().flipX;
        string search = new string($"{_targetObj.name.First()}_Death"); //_targetobjがnull
        _targetPieceAnimatorController.Play(search);
    }

    public void StartStageOutAddForce()
    {
        _targetObj.GetComponent<Collider2D>().enabled = false;
        Rigidbody2D rigidbody2D = _targetObj.GetComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        Vector2 duration = _selectedPieceObj.GetComponent<SpriteRenderer>().flipX ? new Vector2(-100, 100): new Vector2(100, 100);
        _targetPieceAnimatorController.enabled = false;
        rigidbody2D.velocity = duration;
        int destroyTimer = 3;
        Destroy(_targetObj, destroyTimer);
    }
    /// <summary>
    /// 適宜、flipXを反転させるAnimation。ほとんどのAnimationClipで再生時にEventとして呼ばれる
    /// /// </summary>
    public void StartAdjustFlipX()
    {
        //向かっていく方向によって決まる → Directionを取得すれば良い → ここの set をして、AnimationEventに割り当てる
        _selectedPieceObj.GetComponent<SpriteRenderer>().flipX = !_isDirectionRight;
    }

    public void StartAdjustPosition()
    {
        if (_selectedPiece._PieceName == "N")
        {
            Vector3 updatePos = _selectedPieceObj.transform.position;
            updatePos.x += _isDirectionRight? 1.5f : -1.5f;
            _selectedPieceObj.transform.position = updatePos;;
        }
    }
    /// <summary>
    /// RotateRockを指定したポジションにセットし、SetActiveをtrueにする
    /// </summary>
    public void StartRAttackEffect()
    {
        //生成場所 → -3.5 + -3.26 → 4.35 + 5.53, X == 0.24 Y == 1.2
        Vector3 basePos = _selectedPieceObj.transform.position;
        Vector3 adjustPos = new Vector3(0.24f, 1.8f, 0);
        if (!_isDirectionRight)
        {
            adjustPos = new Vector3(-0.24f, 1.8f, 0);
            _RAttackEffectObj.GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            _RAttackEffectObj.GetComponent<SpriteRenderer>().flipX = false;
        }
        adjustPos += basePos;
        _RAttackEffectObj.transform.position = adjustPos;
        _RAttackEffectObj.GetComponent<R_AttackEffect>()._targetPos = _targetObj.transform.position;
        _RAttackEffectObj.SetActive(true);
        //SetActiveがtrueになると岩が回転するAnimationが再生される
    }

    public void StartBAttackEffect()
    { 
        _RAttackEffectObj.GetComponent<SpriteRenderer>().flipX = !_isDirectionRight;
        Vector3 basePos = _targetObj.transform.position;
        _BAttackEffectObj.transform.position = basePos;
        _BAttackEffectObj.SetActive(true);
    }

    public void StartQAttackEffect()
    {
        _QAttckEffectObj.GetComponent<SpriteRenderer>().flipX = !_isDirectionRight;
        Vector3 basePos = _selectedPieceObj.transform.position;
        _QAttckEffectObj.transform.position = basePos;
        _QAttckEffectObj.SetActive(true);
    }
    void EndTurn()
    {
        Squere movedSquere = _targetSquere;
        movedSquere._IsOnPieceObj = _selectedPieceObj; //ここまでにはtargetSquereを移動先の状態にしたい
        if (_enpassantSquere) {_enpassantSquere._IsActiveEnpassant = false;}
        Destroy(_enpassantObj);
        _openSelectableArea.BeforeRendereringClear();
        //Poneが移動した後にアンパッサン・プロモーションの発生を判断する
        if (_selectedPiece._PieceName == "P")
        {
            //被アンパッサン（状況作成側）の処理
            if (Math.Abs(_selectedSquere._SquereTilePos.x - _targetSquere._SquereTilePos.x) == 2)
            {
                CreateEnpassant();
                //_enpassantObj == true
            }
            else if (_targetSquere._SquereID.ToString().Contains("1, 8"))
            {
                _promotionObj = _selectedPieceObj;
                _inGameManager.StartActivePromotionRelay();
                return;
                //プロモーション先の駒をUIで選択したら_inGameManager._IsWhiteを切り替える
            }
        }
        _inGameManager._IsWhite = !_inGameManager._IsWhite; //攻守交代
        //次のターンへ
    }
    /// <summary>
    /// 特殊ルール"アンパッサン"のシチュエーションを作成する処理
    /// </summary>
    /// <param name="_IsWhite"></param>
    public void CreateEnpassant()
    {
        string[] selectedPieceObjName = _selectedPieceObj.name.Split("_"); //P_alphabet_number
        int alphabet = int.Parse(selectedPieceObjName[1]);
        int enpassantNumber;
        if (_inGameManager._IsWhite)
        {
            //WhitePieceのenpassant座標Xは必然的に[2]である
            enpassantNumber = 2;
            _enpassantSquere = _inGameManager._SquereArrays[alphabet][enpassantNumber];
            _enpassantSquere._IsActiveEnpassant = true;
            //enpassantObjの生成・複製・無効化
            _enpassantObj = Instantiate(_collider2DPrefab, _enpassantSquere._SquerePiecePosition, Quaternion.identity);
            _enpassantObj.layer = LayerMask.NameToLayer("Piece");
            _enpassantObj.transform.SetParent(_selectedPieceObj.transform);
            //ennpassantObjの名前をポジションと同一にする
            _enpassantObj.name = new string($"U_{alphabet}_{enpassantNumber}");
        }
        else
        {
            //WhitePieceのenpassant座標Xは必然的に[5]である
            enpassantNumber = 5;
            _enpassantSquere = _inGameManager._SquereArrays[alphabet][enpassantNumber];
            _enpassantSquere._IsActiveEnpassant = true;
            _enpassantObj = Instantiate(_collider2DPrefab, _enpassantSquere._SquerePiecePosition, Quaternion.identity);
            _enpassantObj.layer = LayerMask.NameToLayer("Piece");
            _enpassantObj.transform.SetParent(_selectedPieceObj.transform);
            _enpassantObj.name = new string($"U_{alphabet}_{enpassantNumber}");
        }
    }
}
