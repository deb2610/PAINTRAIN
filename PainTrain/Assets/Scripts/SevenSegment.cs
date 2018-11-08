using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SevenSegment : MonoBehaviour {

    public GameObject leftDigit;
    public GameObject rightDigit;

    private string spritesPrefix = "Sprites/ss";

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void SetDisplay(int n)
    {
        SetLeftDigit((n / 10));
        SetRightDigit(n);
    }

    public void SetLeftDigit(int n)
    {
        int spriteNumber = n % 10;
        Sprite digitSprite = Resources.Load<Sprite>(spritesPrefix + spriteNumber);
        leftDigit.GetComponent<SpriteRenderer>().sprite = digitSprite;
    }

    public void SetRightDigit(int n)
    {
        int spriteNumber = n % 10;
        Sprite digitSprite = Resources.Load<Sprite>(spritesPrefix + spriteNumber);
        rightDigit.GetComponent<SpriteRenderer>().sprite = digitSprite;
    }

    public void TurnOff()
    {
        TurnLeftOff();
        TurnRightOff();
    }

    public void TurnLeftOff()
    {
        Sprite digitSprite = Resources.Load<Sprite>(spritesPrefix + "Off");
        leftDigit.GetComponent<SpriteRenderer>().sprite = digitSprite;
    }

    public void TurnRightOff()
    {
        Sprite digitSprite = Resources.Load<Sprite>(spritesPrefix + "Off");
        rightDigit.GetComponent<SpriteRenderer>().sprite = digitSprite;
    }
}
