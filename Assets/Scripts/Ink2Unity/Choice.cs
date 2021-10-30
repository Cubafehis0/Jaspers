using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ink2Unity
{
    public class Choice
    {
        /// <summary>
        /// ��ǰѡ�������
        /// </summary>
        public Content content;   
        /// <summary>
        /// ��ǰѡ���Ƿ����
        /// </summary>
        public bool canUse;
        /// <summary>
        /// ѡ��򱳾�ɫ����ѡ�������й�
        /// </summary>
        public Color BgColor
        {
            get
            {
                switch (speechArt)
                {
                    case SpeechArt.Cheat:
                        return Color.yellow;
                    case SpeechArt.Persuade:
                        return Color.green;
                    case SpeechArt.Threaten:
                        return Color.red;
                    default:
                        return Color.white;
                }
            }
        }


        /// <summary>
        /// ��ǰѡ���Ƿ�ɼ�
        /// </summary>
        public bool isVisible;

        /// <summary>
        /// ��ǰѡ���Ƿ�����
        /// </summary>
        public bool isLocked;

        //public bool IsLocked
        //{
        //    get
        //    {
        //        return this.isLocked;
        //    }
        //}
        //private bool isLocked;
        //public void Locked()
        //{
        //    isLocked = true;
        //}


        
        /// <summary>
        /// ��ǰѡ��Ļ�������
        /// </summary>
       
        public SpeechArt speechArt;
        
        /// <summary>
        /// ��ǰѡ�������ֵ
        /// </summary>
        public int index;
        public Choice()
        {
            speechArt = SpeechArt.Normal;
            canUse = true;
        }
        public override string ToString()
        {
            string s = "   ";
            return index + s + content.ToString() + s + speechArt.ToString() + s + BgColor + s + canUse;
        }
    }

}