using UnityEngine;
using UnityEngine.Rendering;

public class CharacterSprite : MonoBehaviour
{
    [SerializeField] SortingGroup charaSprite;
    [SerializeField] SpriteRenderer charaImg;
    [SerializeField] SpriteRenderer hierightImg;

    private void Start()
    {
        hierightImg.gameObject.SetActive(false);
    }

    public void SetIndex(int index)
    {
        charaSprite.sortingOrder = index;
    }

    public void SetSprite(Sprite img)
    {
        charaImg.sprite = img;
        hierightImg.sprite = img;
    }
    public Sprite GetSprite()
    {
        return charaImg.sprite;
    }

    public void SetColor(Color color)
    {
        charaImg.color = color;
    }

    public void Heiright(bool flag)
    {
        hierightImg.gameObject.SetActive(flag);
    }
}
