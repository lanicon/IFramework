﻿/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.1
 *UnityVersion:   2017.2.3p3
 *Date:           2019-07-02
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/
using IFramework.Modules;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IFramework.UI
{
    public class UIEventArgs : FrameworkArgs
    {
        public enum Code
        {
            GoBack, GoForward, Push
        }
        public Code code;
        public UIPanel popPanel;
        public UIPanel curPanel;
        public UIPanel pressPanel;
        protected override void OnDataReset()
        {
            popPanel = null;
            curPanel = null;
            pressPanel = null;
        }
    }

    public interface IGroups : IDisposable
    {
        UIPanel FindPanel(string name);
        void InvokeViewState(UIEventArgs arg);
        void Subscribe(UIPanel panel);
        void UnSubscribe(UIPanel panel);
    }
    public interface IPanelLoader
    {
        UIPanel Load(Type type, string name, UILayer layer = UILayer.Common, string path = "");
    }
    public partial class UIModule
    {
        public Canvas Canvas { get; private set; }
        private RectTransform UIRoot;
        public RectTransform BGBG { get; private set; }
        public RectTransform Background { get; private set; }
        public RectTransform AnimationUnderPage { get; private set; }
        public RectTransform Common { get; private set; }
        public RectTransform AnimationOnPage { get; private set; }
        public RectTransform PopUp { get; private set; }
        public RectTransform Guide { get; private set; }
        public RectTransform Toast { get; private set; }
        public RectTransform Top { get; private set; }
        public RectTransform TopTop { get; private set; }
        public RectTransform UICamera { get; private set; }
        private RectTransform InitTransform(string name)
        {
            GameObject go = new GameObject(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetParent(Canvas.transform);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.localPosition = Vector3.zero;
            rect.sizeDelta = Vector3.zero;
            return rect;
        }
        private void InitTransform()
        {
            UIRoot = new GameObject(name).AddComponent<RectTransform>();
            Canvas = UIRoot.gameObject.AddComponent<Canvas>();
            UIRoot.gameObject.AddComponent<CanvasScaler>();
            UIRoot.gameObject.AddComponent<GraphicRaycaster>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            BGBG = InitTransform("BGBG");
            Background = InitTransform("Background");
            AnimationUnderPage = InitTransform("AnimationUnderPage");
            Common = InitTransform("Common");
            AnimationOnPage = InitTransform("AnimationOnPage");
            PopUp = InitTransform("PopUp");
            Guide = InitTransform("Guide");
            Toast = InitTransform("Toast");
            Top = InitTransform("Top");
            TopTop = InitTransform("TopTop");
            UICamera = InitTransform("UICamera");
        }
        public void SetCamera(Camera ca, bool isLast = true, int index = -1)
        {
            UICamera.SetChildWithIndex(ca.transform, !isLast ? index : UICamera.childCount);
        }
        public void SetParent(UIPanel ui, bool isLast = true, int index = -1)
        {
            switch (ui.layer)
            {
                case UILayer.BGBG:
                    BGBG.SetChildWithIndex(ui.transform, !isLast ? index : BGBG.childCount);
                    break;
                case UILayer.Background:
                    Background.SetChildWithIndex(ui.transform, !isLast ? index : Background.childCount);
                    break;
                case UILayer.AnimationUnderPage:
                    AnimationUnderPage.SetChildWithIndex(ui.transform, !isLast ? index : AnimationUnderPage.childCount);
                    break;
                case UILayer.Common:
                    Common.SetChildWithIndex(ui.transform, !isLast ? index : Common.childCount);
                    break;
                case UILayer.AnimationOnPage:
                    AnimationOnPage.SetChildWithIndex(ui.transform, !isLast ? index : AnimationOnPage.childCount);
                    break;
                case UILayer.PopUp:
                    PopUp.SetChildWithIndex(ui.transform, !isLast ? index : PopUp.childCount);
                    break;
                case UILayer.Guide:
                    Guide.SetChildWithIndex(ui.transform, !isLast ? index : Guide.childCount);
                    break;
                case UILayer.Toast:
                    Toast.SetChildWithIndex(ui.transform, !isLast ? index : Toast.childCount);
                    break;
                case UILayer.Top:
                    Top.SetChildWithIndex(ui.transform, !isLast ? index : Top.childCount);
                    break;
                case UILayer.TopTop:
                    TopTop.SetChildWithIndex(ui.transform, !isLast ? index : TopTop.childCount);
                    break;
                default:
                    break;
            }
            ui.transform.LocalIdentity();
        }
    }
    public partial class UIModule
    {
        private List<IPanelLoader> _loaders;
        public int loaderCount { get { return _loaders.Count; } }

        public Stack<UIPanel> stack;
        public Stack<UIPanel> memory;
        public int stackCount { get { return stack.Count; } }
        public int memoryCount { get { return memory.Count; } }
        public bool ExistInStack(UIPanel panel) { return stack.Contains(panel); }
        public bool ExistInMemory(UIPanel panel) { return memory.Contains(panel); }
        public bool Exist(UIPanel panel) { return ExistInMemory(panel) || ExistInStack(panel); }
        public UIPanel current
        {
            get
            {
                if (stack.Count == 0)
                    return null;
                return stack.Peek();
            }
        }
        public UIPanel MemoryPeek()
        {
            if (memory.Count == 0)
                return null;
            return memory.Peek();
        }
    }
    public partial class UIModule : FrameworkModule
    {
        private IGroups _groups;
        public override int priority { get { return 80; } }

        protected override void Awake()
        {
            InitTransform();
            stack = new Stack<UIPanel>();
            memory = new Stack<UIPanel>();
            _loaders = new List<IPanelLoader>();
        }
        protected override void OnDispose()
        {
            stack.Clear();
            memory.Clear();
            _loaders.Clear();
            if (_groups != null)
                _groups.Dispose();
            if (Canvas != null)
                GameObject.Destroy(Canvas.gameObject);
        }

        public void AddLoader(IPanelLoader loader)
        {
            _loaders.Add(loader);
        }
        public void SetGroups(IGroups groups)
        {
            this._groups = groups;
        }

        public UIPanel Load(Type type, string name, UILayer layer = UILayer.Common, string path = "")
        {
            if (_groups == null)
                throw new Exception("Please Set ModuleType First");
            UIPanel ui = default(UIPanel);
            if (_loaders == null || loaderCount == 0)
            {
                Log.E("NO UILoader");
                return ui;
            }
            for (int i = 0; i < _loaders.Count; i++)
            {
                var result = _loaders[i].Load(type, name, layer, path);
                if (result == null) continue;
                ui = result;
                ui = GameObject.Instantiate(ui);
                ui.layer = layer;
                SetParent(ui);
                ui.name = name;
                ui.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                _groups.Subscribe(ui);
                return ui;
            }
            Log.E(string.Format("NO ui Type: {0}  Path: {1}  Layer: {2}  Name: {3}", type, path, layer, name));
            return ui;
        }
        public T Load<T>(string name, UILayer layer = UILayer.Common, string path = "") where T : UIPanel
        {
            return (T)Load(typeof(T), name, layer, path);
        }
        public bool HaveLoad(string panelName)
        {
            return _groups.FindPanel(panelName) != null;
        }

        public void Push(UIPanel ui)
        {
            UIEventArgs arg = UIEventArgs.Allocate<UIEventArgs>(this.container.env.envType);
            arg.code = UIEventArgs.Code.Push;
            if (stackCount > 0)
                arg.pressPanel = current;
            arg.curPanel = ui;
            stack.Push(ui);
            InvokeViewState(arg);
            arg.Recyle();
            if (memory.Count > 0) ClearCache();
        }
        public void GoForWard()
        {
            UIEventArgs arg = UIEventArgs.Allocate<UIEventArgs>(this.container.env.envType);
            arg.code = UIEventArgs.Code.GoForward;
            if (memoryCount <= 0) return;
            if (stackCount > 0)
                arg.pressPanel = current;
            var ui = memory.Pop();
            arg.curPanel = ui;
            stack.Push(ui);
            InvokeViewState(arg);
            arg.Recyle();
        }
        public void GoBack()
        {
            UIEventArgs arg = UIEventArgs.Allocate<UIEventArgs>(this.container.env.envType);
            arg.code = UIEventArgs.Code.GoBack;
            if (stackCount <= 0) return;
            var ui = stack.Pop();
            arg.popPanel = ui;
            memory.Push(ui);
            if (stackCount > 0)
                arg.curPanel = current;
            InvokeViewState(arg);
            arg.Recyle();
        }
        public void ClearCache()
        {
            while (memory.Count != 0)
            {
                UIPanel p = memory.Pop();
                if (p != null && !ExistInStack(p))
                {
                    _groups.UnSubscribe(p);
                }
            }
        }
        private void InvokeViewState(UIEventArgs arg)
        {
            _groups.InvokeViewState(arg);
        }

        public UIPanel Get(Type type, string name, UILayer layer = UILayer.Common, string path = "")
        {
            //if (UICache.Count > 0) ClearCache(arg);
            if (current != null && current.name == name && current.GetType() == type)
                return current;
            var panel = _groups.FindPanel(name);
            if (panel == null)
                panel = Load(type, name, layer, path);
            Push(panel);
            return panel;
        }
        public T Get<T>(string name, UILayer layer = UILayer.Common, string path = "") where T : UIPanel
        {
            return (T)Get(typeof(T), name, layer, path);
        }
    }
}
