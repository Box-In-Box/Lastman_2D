using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    List<Resolution> resolutions = new List<Resolution>();
    public Dropdown resolutionDropdown;

    FullScreenMode screenMode;
    public Toggle fullScreenToggle;

    public int resolutionNum = 0;

    void Start()
    {
        InitUI();
    }

    void InitUI()
    {   

        for (int i = 0; i < Screen.resolutions.Length; i++) {
            if (Screen.resolutions[i].refreshRate == 60) {
                if (Screen.resolutions[i].width == 1280 && Screen.resolutions[i].height == 720)
                    resolutions.Add(Screen.resolutions[i]);
                else if (Screen.resolutions[i].width == 1920 && Screen.resolutions[i].height == 1080)
                    resolutions.Add(Screen.resolutions[i]);
                else if (Screen.resolutions[i].width == 2560 && Screen.resolutions[i].height == 1440)
                    resolutions.Add(Screen.resolutions[i]);
                else if (Screen.resolutions[i].width == 3840 && Screen.resolutions[i].height == 2160)
                    resolutions.Add(Screen.resolutions[i]);
            }
        }
        
        resolutionDropdown.options.Clear();

        int optionNum = 0;
        foreach (Resolution item in resolutions) {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = item.width + "X" + item.height + " " + item.refreshRate + "hz";
            resolutionDropdown.options.Add(option);

            if (item.width == Screen.width && item.height == Screen.height)
                resolutionDropdown.value = optionNum;
            
            optionNum++;
        }
        resolutionDropdown.RefreshShownValue();

        if (resolutionDropdown.options.Count == 0) { //에디터 에러 방지용
            resolutions.Add(Screen.resolutions[0]);
        }

        fullScreenToggle.isOn = Screen.fullScreenMode.Equals(FullScreenMode.FullScreenWindow) ? true : false;
    }

    public void DropboxOptionChange(int num)
    {
        resolutionNum = num;

        ChangeResolution();
    }

    public void FullScreenBtn(bool isFull)
    {
        screenMode = isFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        ChangeResolution();
    }

    public void ChangeResolution()
    {
        Screen.SetResolution(resolutions[resolutionNum].width,
            resolutions[resolutionNum].height,
            screenMode);
    }
}
