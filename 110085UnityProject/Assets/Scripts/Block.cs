using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Block : MonoBehaviour
{
    public Image img;

    private int _type;
    public int type
    {
        get { return _type; }
        set { 
            _type = value;
        }
    }

    private int _x;
    public int x
    {
        get { return _x; }
        set { _x = value; }
    }

    private int _y;
    public int y
    {
        get { return _y; }
        set { _y = value; }
    }

    public int index
    {
        get;set;
    }

    private bool _add = false;
    public bool add
    {
        get
        {
            return _add;
        }
        set
        {
            _add = value;
        }
    }

    public void setSprite(Sprite sprite)
    {
        img.sprite = sprite;
        img.SetNativeSize();
    }

    public void disappear(bool back, float delay = 0)
    {
        Vector3 localPos = this.transform.localPosition;
        Vector3 targetPos;
        if (back)
        {
            this.transform.SetAsFirstSibling();
            targetPos = new Vector3(localPos.x - 50, localPos.y + 50, 0);
        }            
        else
        {
            this.transform.SetAsLastSibling();
            targetPos = new Vector3(localPos.x +20, localPos.y - 20, 0);
        }
        if(add)
        {
            Game.gameStatus = GameStatus.Add;
        }
        if (back)
            this.transform.SetParent(this.transform.parent.parent.Find("outBlockContainer"));
        this.transform.DOLocalMove(targetPos, 0.3f).OnComplete(() =>
        {
            if(add)
            {
                SendMessageUpwards("addBlock", null, SendMessageOptions.RequireReceiver);
            }
           this.transform.DOLocalMoveY(localPos.y - 400, 0.5f).SetEase(Ease.InCirc).OnComplete(()=>
           {
               GameObject.Destroy(this.gameObject);
           });
        }).SetEase(Ease.OutCirc).SetDelay(delay);
    }

    public void dotweenMoveY(float y)
    {
        transform.DOKill();
        transform.DOLocalMoveY(y, 0.3f).SetDelay(0.2f);
    }

    public void setAdd()
    {
        add = true;
        transform.Find("spAdd").gameObject.SetActive(true);
    }
}
