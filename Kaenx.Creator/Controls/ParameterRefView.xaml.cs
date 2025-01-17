using Kaenx.Creator.Classes;
using Kaenx.Creator.Models;
using Kaenx.Creator.Models.Dynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kaenx.Creator.Controls
{
    public partial class ParameterRefView : UserControl, IFilterable, ISelectable
    {
        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register("Version", typeof(AppVersion), typeof(ParameterRefView), new PropertyMetadata(OnModuleChangedCallback));
        public static readonly DependencyProperty ModuleProperty = DependencyProperty.Register("Module", typeof(IVersionBase), typeof(ParameterRefView), new PropertyMetadata(OnModuleChangedCallback));
        public AppVersion Version {
            get { return (AppVersion)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }
        public IVersionBase Module {
            get { return (IVersionBase)GetValue(ModuleProperty); }
            set { SetValue(ModuleProperty, value); }
        }

        private TextFilter _filter;
        private object _selectedItem = -1;

        public void FilterShow()
        {
            _filter.Show();
            ParamRefList.SelectedItem = _selectedItem;
        }

        public void FilterHide()
        {
            _filter.Hide();
            _selectedItem = ParamRefList.SelectedItem;
            ParamRefList.SelectedItem = null;
        }

        public void ShowItem(object item)
        {
            ParamRefList.ScrollIntoView(item);
            ParamRefList.SelectedItem = item;
        }

        public ParameterRefView()
		{
            InitializeComponent();
        }

        private static void OnModuleChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ParameterRefView)?.OnModuleChanged(e);
        }

        protected virtual void OnModuleChanged(DependencyPropertyChangedEventArgs e)
        {
            if(e.OldValue != null)
                (e.OldValue as IVersionBase).ParameterRefs.CollectionChanged -= RefsChanged;

            if(e.NewValue != null)
            {
                (e.NewValue as IVersionBase).ParameterRefs.CollectionChanged += RefsChanged;
                _filter = new TextFilter((e.NewValue as IVersionBase).ParameterRefs, query);
            }

        }
        
        private void RefsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.OldItems != null && Module != null)
            {
                foreach(ParameterRef pref in e.OldItems)
                    DeleteDynamicRef(Module.Dynamics[0], pref);
            }
        }

        private void ClickAdd(object sender, RoutedEventArgs e)
        {
            ParameterRef pref = new ParameterRef() { UId = AutoHelper.GetNextFreeUId(Module.ParameterRefs) };
            foreach(Language lang in Version.Languages)
            {
                pref.Text.Add(new Translation(lang, ""));
                pref.Suffix.Add(new Translation(lang, ""));
            }

            Module.ParameterRefs.Add(pref);
            ParamRefList.ScrollIntoView(pref);
            ParamRefList.SelectedItem = pref;
        }

        private void ClickRemove(object sender, RoutedEventArgs e)
        {
            ParameterRef pref = ParamRefList.SelectedItem as ParameterRef;

            if(CheckDynamicRef(Module.Dynamics[0], pref)
                && MessageBoxResult.No == MessageBox.Show("Dieser ParameterRef wird mindestens ein mal im Dynamic benutzt. Wirklich löschen?", "ParameterRef löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning))
                    return;

            Module.ParameterRefs.Remove(pref);
        }

        private bool CheckDynamicRef(IDynItems item, ParameterRef pref)
        {
            bool flag = false;

            switch(item)
            {
                case DynChannel dc:
                    if(dc.UseTextParameter && dc.ParameterRefObject == pref)
                        flag = true;
                    break;

                case DynParaBlock dpb:
                    if(dpb.UseParameterRef && dpb.ParameterRefObject == pref)
                        flag = true;
                    if(dpb.UseTextParameter && dpb.TextRefObject == pref)
                        flag = true;
                    break;

                case DynParameter dp:
                    if(dp.ParameterRefObject == pref)
                        flag = true;
                    break;

                case IDynChoose dch:
                    if(dch.ParameterRefObject == pref)
                        flag = true;
                    break;
            }

            if(flag) return true;

            if(item.Items != null)
                foreach(IDynItems ditem in item.Items)
                    if(CheckDynamicRef(ditem, pref))
                        flag = true;

            return flag;
        }

        private void DeleteDynamicRef(IDynItems item, ParameterRef pref)
        {
            switch(item)
            {
                case DynChannel dc:
                    if(dc.UseTextParameter && dc.ParameterRefObject == pref)
                        dc.ParameterRefObject = null;
                    break;

                case DynParaBlock dpb:
                    if(dpb.UseParameterRef && dpb.ParameterRefObject == pref)
                        dpb.ParameterRefObject = null;
                    if(dpb.UseTextParameter && dpb.TextRefObject == pref)
                        dpb.TextRefObject = null;
                    break;

                case DynParameter dp:
                    if(dp.ParameterRefObject == pref)
                        dp.ParameterRefObject = null;
                    break;

                case IDynChoose dch:
                    if(dch.ParameterRefObject == pref)
                        dch.ParameterRefObject = null;
                    break;
            }

            if(item.Items != null)
                foreach(IDynItems ditem in item.Items)
                    DeleteDynamicRef(ditem, pref);
        }

        private void ResetId(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as ParameterRef).Id = -1;
        }
        
        private void ManuelId(object sender, RoutedEventArgs e)
        {
            PromptDialog diag = new PromptDialog("Neue ParameterRef ID", "ID Manuell");
            if(diag.ShowDialog() == true)
            {
                long id;
                if(!long.TryParse(diag.Answer, out id))
                {
                    MessageBox.Show("Bitte geben Sie eine Ganzzahl ein.", "Eingabefehler");
                    return;
                }
                ParameterRef ele = Module.ParameterRefs.SingleOrDefault(p => p.Id == id);
                if(ele != null)
                {
                    MessageBox.Show($"Die ID {id} wird bereits von ParameterRef {ele.Name} verwendet.", "Doppelte ID");
                    return;
                }
                ((sender as Button).DataContext as Models.ParameterRef).Id = id;
            }
        }
    
        private void AutoId(object sender, RoutedEventArgs e)
        {
            Models.ParameterRef ele = (sender as Button).DataContext as Models.ParameterRef;
            long oldId = ele.Id;
            ele.Id = -1;
            ele.Id = AutoHelper.GetNextFreeId(Module, "ParameterRefs");
            if(ele.Id == oldId)
                MessageBox.Show("Das Element hat bereits die erste freie ID", "Automatische ID");
        }
    }
}