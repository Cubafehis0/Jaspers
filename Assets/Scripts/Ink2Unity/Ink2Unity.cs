using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

/// <summary>
/// 工具类，封装了一些Ink提供的API
/// </summary>
namespace Ink2Unity
{
    /// <summary>
    /// 传入Ink的TextAsset来创建一个Ink2Unity实例
    /// </summary>
    public class InkStory
    {
        private static InkStory _nowStory;
        public static InkStory NowStory
        {
            get
            {
                return _nowStory;
            }
            private set
            {
                _nowStory = value;
            }
        }
        Story story;
        Player player;
        List<Choice> choicesList;
        IPlayerStateChange cardGame;
        public InkStory(TextAsset inkJSON)
        {
            story = new Story(inkJSON.text);
            //player = CardGameManager.Instance.player;
            player = new Player();
            BindExternalFunction();
            NowStory = this;
            cardGame = CardGameManager.Instance;
        }
        public void BindExternalFunction()
        {
            story.BindExternalFunction("InnIsIn", (int l, int r) => { return player.Inside >= l && player.Inside < r; });
            story.BindExternalFunction("ExtIsIn", (int l, int r) => { return player.Outside >= l && player.Outside < r; });
            story.BindExternalFunction("LgcIsIn", (int l, int r) => { return player.Logic >= l && player.Logic < r; });
            story.BindExternalFunction("SptIsIn", (int l, int r) => { return player.Passion >= l && player.Passion < r; });
            story.BindExternalFunction("MrlIsIn", (int l, int r) => { return player.Moral >= l && player.Moral < r; });
            story.BindExternalFunction("UtcIsIn", (int l, int r) => { return player.Unethic >= l && player.Unethic < r; });
            story.BindExternalFunction("RdbIsIn", (int l, int r) => { return player.Detour >= l && player.Detour < r; });
            story.BindExternalFunction("AgsIsIn", (int l, int r) => { return player.Strong >= l && player.Strong < r; });
        }
        /// <summary>
        /// 故事是否可继续读取内容(Content)
        /// </summary>
        public bool CanCoutinue
        {
            get
            {
                return story.canContinue;
            }
        }
        /// <summary>
        /// 玩家进行操作（打牌，选择）后来调用该函数以更新Ink的状态
        /// </summary>
        public void UpdateInk()
        {
            UpdateInkPlayerInfo();
        }
        /// <summary>
        ///   获取当前内容  
        /// </summary>
        public Content CurrentContent()
        {
            string ct;
            //避免表达式的情况
            while ((ct = story.Continue()) == "") ;
            UpdateInkPlayerInfo();
            Content rs = new Content(ct);
            List<string> tags = story.currentTags;
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    string name, value;
                    TagHandle.GetPropertyNameAndValue(tag, out name, out value);
                    ParseValue(rs, name, value);
                }
            }
            if (rs.stateChange != null)
                cardGame.StateChange(rs.stateChange,rs.changeTurn);
            return rs;
        }
        public bool IsFinished
        {
            get
            {
                return !story.canContinue && story.currentChoices.Count == 0;
            }
        }
        /// <summary>
        /// 获取当前所有的选项
        /// </summary>
        /// <returns>选项的集合</returns>
        public List<Choice> CurrentChoices()
        {
            List<Choice> rs = new List<Choice>();
            var choicesContent = story.currentChoices;
            for (int i = 0; i < choicesContent.Count; i++)
            {
                string c;
                List<string> tags = TagHandle.ChoiceCurrentTags(choicesContent[i].text, out c);
                Choice choice = new Choice();
                choice.content = new Content(c);
                choice.index = choicesContent[i].index;
                if (tags != null)
                {
                    foreach (var tag in tags)
                    {
                        string name, value;
                        TagHandle.GetPropertyNameAndValue(tag, out name, out value);
                        ParseValue(choice, name, value);
                    }
                }
                rs.Add(choice);
            }
            choicesList = rs;
            return rs;
        }
        /// <summary>
        /// 根据索引值来进行选择，会返回选择选项后出现的文本，如果没有对应文本，则返回空
        /// </summary>
        /// <param name="index">当前选项的索引值</param>
        public Content SelectChoice(int index)
        {
            story.ChooseChoiceIndex(index);
            Choice cs = choicesList[index];
            if(cs.judgeValue!=null)
            {
                bool success = TalkJudge(cs.judgeValue);
                UpdateInkVariableByName<bool>("judgeSuccess", success);
                if(success)
                {
                    if(cs.success_desc!=0)
                        cardGame.PressureChange(-cs.success_desc);
                }
                else
                {   
                    if(cs.fail_add!=0)
                        cardGame.PressureChange(cs.fail_add);
                }
            }
            var sc = cs.content.stateChange;
            if (sc != null)
                cardGame.StateChange(sc,cs.content.changeTurn);
            UpdateInk();
            ///
            // 进行判定的过程
            ///忽略原本内容
            Content c = CurrentContent();
            //采用定义的新内容
            c = CurrentContent();
            if (c.richText != "NIL")
                return c;
            return null;

        }
        /// <summary>
        /// 根据现选项来进行选择，会返回选择选项后出现的文本，如果没有文本返回则为空
        /// </summary>
        /// <param name="choice">当前选项</param>
        public Content SelectChoice(Choice choice)
        {
            return SelectChoice(choice.index);
        }
        /// <summary>
        /// 获取当前选项个数
        /// </summary>
        public int CurrentChoicesCount
        {
            get
            {
                return CurrentChoices().Count;
            }
        }
        
        public string NowState2Json()
        {
            return story.state.ToJson();
        }
        public void LoadStory(string state)
        {
            story.state.LoadJson(state);
        }
        //private void StateChange(int i, int l, int m, int r, int t)
        //{
        //    int[] p = { i, l, m, r };
        //    for (int j = 0; j < 4; j++)
        //    {
        //        player.data[j] += p[j];
        //    }
        //    if (t != -1)
        //    {
        //        //延迟buff
        //    }
        //}

        private void StateSet(int i, int l, int m, int r)
        {
            int[] p = { i, l, m, r };
            for (int j = 0; j < 4; j++)
                player.data[j] = p[j];
        }
        private bool  ChoiceCanUse(List<int> values)
        {
            for (int j = 0; j < 4; j++)
            {
                if (values[j] > 0)
                {
                    if (player.data[j]< values[j])
                        return false;
                }
                else if (values[j] < 0)
                {
                    if (player.data[j]> values[j])
                        return false;
                }
            }
            return true;
        }
        private bool TalkJudge(List<int> values)
        {
            int r = GetRandomJudge();
            for(int j=0;j<4;j++)
            {
                if(values[j]>0)
                {
                    if (player.data[j] + r < values[j])
                        return false;
                }
                else if(values[j]<0)
                {
                    if (player.data[j] - r > values[j])
                        return false;
                }
            }
            return true;
        }
        //不同判定补正的概率
        float[] jp = { 0.05f, 0.2f, 0.5f, 0.2f, 0.05f };
        int GetRandomJudge()
        {
            float a = Random.value;
            float l = 0, r = 0;
            for (int i = 0; i < jp.Length; i++)
            {
                l = r;
                r = r + jp[i];
                if (a >= l && a < r)
                    return i;
            }
            return 0;
        }
        private void UpdateInkPlayerInfo()
        {
            story.variablesState["inn"] = player.Inside;
            story.variablesState["ext"] = player.Outside;
            story.variablesState["lgc"] = player.Logic;
            story.variablesState["spt"] = player.Passion;
            story.variablesState["mrl"] = player.Moral;
            story.variablesState["utc"] = player.Unethic;
            story.variablesState["rdb"] = player.Detour;
            story.variablesState["ags"] = player.Strong;
        }
        private void UpdateInkVariableByName<T>(string name,T value)
        {
            story.variablesState[name] = value;
        }
       
        void ParseValue(Content content,string name,string value)
        {
            switch (name)
            {
                case "Speaker":
                    content.speaker = TagHandle.ParseSpeaker(value);
                    return;
                case "StateChange":
                    List<int> a = TagHandle.ParseArray(value);
                    content.stateChange = a.GetRange(0, 4);
                    content.changeTurn = a[4];
                    return;
                default:
                    Debug.LogError("无法识别的标签类型："+name+":"+value);
                    return;
            }
        }
        void ParseValue(Choice choice, string name, string value)
        {
            switch (name)
            {
                case "Speaker":
                    choice.content.speaker = TagHandle.ParseSpeaker(value);
                    return;
                case "CanUse":
                    List<int> values = TagHandle.ParseArray(value);
                    choice.judgeValue = values;
                    choice.canUse = ChoiceCanUse(values);
                    return;
                case "SpeechArt":
                    choice.speechArt = TagHandle.ParseSpeechArt(value);
                    return;
                case "Success":
                    choice.success_desc = int.Parse(value);
                    return;
                case "Fail":
                    choice.fail_add = int.Parse(value);
                    return;
                case "StateChange":
                    List<int> a = TagHandle.ParseArray(value);
                    choice.content.stateChange = a.GetRange(0, 4);
                    choice.content.changeTurn = a[4];
                    return;
                default:
                    Debug.LogError("无法识别的标签类型：" + name + ":" + value);
                    return;
            }
        }
    }

}
