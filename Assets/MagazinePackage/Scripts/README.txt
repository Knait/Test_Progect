using UnityEngine.InputSystem; //ВАЖНО: использует new input system для определения с чего производится ввод!!!
Если у вас старая, то замените условия определения на свою систему

Пример как записываю я.
ID в магазине равны нумерации в массиве

[SerializeField] private int animNumber;

[Header("Скины сюда!")]
[SerializeField] private Animator[] anim;

на старте уровня.
        if (!PlayerPrefs.HasKey("BodySkin_ID"))
            PlayerPrefs.SetInt("BodySkin_ID", 1);
        else
        for (int i = 1; i < anim.Length; i++)
            if (i == PlayerPrefs.GetInt("BodySkin_ID"))
                anim[i].gameObject.SetActive(true);
            else
                anim[i].gameObject.SetActive(false);

        animNumber = PlayerPrefs.GetInt("BodySkin_ID");


if (anim[animNumber].GetBool("Run")) anim[animNumber].SetBool("Run", false);