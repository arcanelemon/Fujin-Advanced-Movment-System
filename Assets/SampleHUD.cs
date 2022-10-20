using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SampleHUD : MonoBehaviour
{
    #region Variables

    [SerializeField]
    TextMeshProUGUI velocityTextUGUI;
    [SerializeField]
    Image velocityMeterFillImage;

    Fujin.Movement.PlayerMotor player;

    #endregion Variables
    
    #region Engine

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindObjectOfType<Fujin.Movement.PlayerMotor>();
    }

    // Update is called once per frame
    void Update()
    {
        int velocity = (int)new Vector3(player.Velocity.x, 0, player.Velocity.z).magnitude;

        velocityTextUGUI.text = velocity.ToString() + " m/s";
        if (velocity == 0)
            velocityTextUGUI.color = Color.red;
        else
            velocityTextUGUI.color = Color.white;

        velocityMeterFillImage.fillAmount = velocity / player.physics.maxVelocity.x;
    }

    #endregion Engine
}
