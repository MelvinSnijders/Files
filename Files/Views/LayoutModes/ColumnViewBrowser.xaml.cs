﻿using Files.EventArguments;
using Files.Filesystem;
using Files.Helpers;
using Files.Helpers.XamlHelpers;
using Files.Interacts;
using Files.UserControls.Selection;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Files.Views.LayoutModes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ColumnViewBrowser : BaseLayout
    {
        public static IShellPage columnparent;
        private NavigationArguments parameters;
        private ListViewItem listViewItem;
        public static ColumnViewBrowser ColumnViewBrowser1;

        public ColumnViewBrowser() : base()
        {
            this.InitializeComponent();
            ColumnViewBrowser1 = this;
            ColumnViewBase.ItemInvoked += ColumnViewBase_ItemInvoked;
            ColumnViewBase.UnFocusPreviousListView += ColumnViewBase_UnFocusPreviousListView;
            ColumnViewBase.DismissColumn += ColumnViewBase_DismissColumn;
            //this.DataContext = this;
            var selectionRectangle = RectangleSelection.Create(FileList, SelectionRectangle, FileList_SelectionChanged);
        }

        protected override void HookEvents()
        {
            UnhookEvents();
            ItemManipulationModel.FocusFileListInvoked += ItemManipulationModel_FocusFileListInvoked;
            ItemManipulationModel.SelectAllItemsInvoked += ItemManipulationModel_SelectAllItemsInvoked;
            ItemManipulationModel.ClearSelectionInvoked += ItemManipulationModel_ClearSelectionInvoked;
            ItemManipulationModel.InvertSelectionInvoked += ItemManipulationModel_InvertSelectionInvoked;
            ItemManipulationModel.AddSelectedItemInvoked += ItemManipulationModel_AddSelectedItemInvoked;
            ItemManipulationModel.RemoveSelectedItemInvoked += ItemManipulationModel_RemoveSelectedItemInvoked;
            ItemManipulationModel.FocusSelectedItemsInvoked += ItemManipulationModel_FocusSelectedItemsInvoked;
            ItemManipulationModel.StartRenameItemInvoked += ItemManipulationModel_StartRenameItemInvoked;
            ItemManipulationModel.ScrollIntoViewInvoked += ItemManipulationModel_ScrollIntoViewInvoked;
        }

        private void ItemManipulationModel_ScrollIntoViewInvoked(object sender, ListedItem e)
        {
            try
            {
                FileList.ScrollIntoView(e, ScrollIntoViewAlignment.Default);
            }
            catch (Exception)
            {
                // Catch error where row index could not be found
            }
        }

        private void ItemManipulationModel_StartRenameItemInvoked(object sender, EventArgs e)
        {
            StartRenameItem();
        }

        private void ItemManipulationModel_FocusSelectedItemsInvoked(object sender, EventArgs e)
        {
            FileList.ScrollIntoView(FileList.Items.Last());
        }

        private void ItemManipulationModel_AddSelectedItemInvoked(object sender, ListedItem e)
        {
            FileList?.SelectedItems.Add(e);
        }

        private void ItemManipulationModel_RemoveSelectedItemInvoked(object sender, ListedItem e)
        {
            FileList?.SelectedItems.Remove(e);
        }

        private void ItemManipulationModel_InvertSelectionInvoked(object sender, EventArgs e)
        {
            if (!IsLastColumnBase)
            {
                var c = ColumnHost.ActiveBlades.Last();
                ((c.Content as Frame).Content as ColumnShellPage).NavToolbarViewModel.InvertContentPageSelctionCommand.Execute(null);
                return;
            }

            if (SelectedItems.Count < GetAllItems().Cast<ListedItem>().Count() / 2)
            {
                var oldSelectedItems = SelectedItems.ToList();
                ItemManipulationModel.SelectAllItems();
                ItemManipulationModel.RemoveSelectedItems(oldSelectedItems);
            }
            else
            {
                List<ListedItem> newSelectedItems = GetAllItems()
                    .Cast<ListedItem>()
                    .Except(SelectedItems)
                    .ToList();

                ItemManipulationModel.SetSelectedItems(newSelectedItems);
            }
        }

        private void ItemManipulationModel_ClearSelectionInvoked(object sender, EventArgs e)
        {
            if (IsLastColumnBase)
            {
                FileList.SelectedItems.Clear();
            }
            else
            {
                var c = ColumnHost.ActiveBlades.Last();
                ((c.Content as Frame).Content as ColumnShellPage).NavToolbarViewModel.ClearContentPageSelectionCommand.Execute(null);
            }
        }

        private void ItemManipulationModel_SelectAllItemsInvoked(object sender, EventArgs e)
        {
            if (IsLastColumnBase)
            {
                FileList.SelectAll();
            } else
            {
                var c = ColumnHost.ActiveBlades.Last();
                ((c.Content as Frame).Content as ColumnShellPage).NavToolbarViewModel.SelectAllContentPageItemsCommand.Execute(null);
            }
        }

        private void ItemManipulationModel_FocusFileListInvoked(object sender, EventArgs e)
        {
            FileList.Focus(FocusState.Programmatic);
        }

        protected override void UnhookEvents()
        {
            if (ItemManipulationModel != null)
            {
                ItemManipulationModel.FocusFileListInvoked -= ItemManipulationModel_FocusFileListInvoked;
                ItemManipulationModel.SelectAllItemsInvoked -= ItemManipulationModel_SelectAllItemsInvoked;
                ItemManipulationModel.ClearSelectionInvoked -= ItemManipulationModel_ClearSelectionInvoked;
                ItemManipulationModel.InvertSelectionInvoked -= ItemManipulationModel_InvertSelectionInvoked;
                ItemManipulationModel.AddSelectedItemInvoked -= ItemManipulationModel_AddSelectedItemInvoked;
                ItemManipulationModel.RemoveSelectedItemInvoked -= ItemManipulationModel_RemoveSelectedItemInvoked;
                ItemManipulationModel.FocusSelectedItemsInvoked -= ItemManipulationModel_FocusSelectedItemsInvoked;
                ItemManipulationModel.StartRenameItemInvoked -= ItemManipulationModel_StartRenameItemInvoked;
                ItemManipulationModel.ScrollIntoViewInvoked -= ItemManipulationModel_ScrollIntoViewInvoked;
            }
        }

        private void ColumnViewBase_DismissColumn(object sender, EventArgs e)
        {
            DismissOtherBlades(sender as ListView);
        }

        private void ColumnViewBase_UnFocusPreviousListView(object sender, EventArgs e)
        {
            var list = sender as ListView;
            var blade = list.FindAscendant<BladeItem>();
            var index = ColumnHost.ActiveBlades.IndexOf(blade) - 1;
            if (index == 0)
            {
                //_ = VisualStateManager.GoToState(listViewItem, "NotCurrentItem", true);
            }
            else
            {
                try
                {
                    var listview = ColumnHost.ActiveBlades[index].FindDescendant("FileList") as ListView;
                    ListViewItem listViewItem2 = listview.ContainerFromItem((listview.SelectedItem) as ListedItem) as ListViewItem;
                }
                catch
                {
                }
            }
        }

        private void FileList_GotFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Got focus");
        }

        private void ColumnViewBase_ItemInvoked(object sender, EventArgs e)
        {
            var column = sender as ColumnParam;

            var frame = new Frame();
            var newblade = new BladeItem();
            newblade.Content = frame;
            ColumnHost.Items.Add(newblade);
            //pane.NavigateWithArguments(typeof(ColumnViewBase), new NavigationArguments()
            //{
            //    NavPathParam = item.ItemPath,
            //    AssociatedTabInstance = ParentShellPageInstance
            //});

            frame.Navigate(typeof(ColumnShellPage), new ColumnParam
            {
                Column = ColumnHost.ActiveBlades.IndexOf(newblade),
                Path = column.Path
            });
        }

        private void ListViewTextBoxItemName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (FilesystemHelpers.ContainsRestrictedCharacters(textBox.Text))
            {
                FileNameTeachingTip.Visibility = Visibility.Visible;
                FileNameTeachingTip.IsOpen = true;
            }
            else
            {
                if (FileNameTeachingTip.IsOpen == true)
                {
                    FileNameTeachingTip.IsOpen = false;
                    FileNameTeachingTip.Visibility = Visibility.Collapsed;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);
            //var param = (eventArgs.Parameter as NavigationArguments);
            //NavParam = param.NavPathParam;
            //var viewmodel = new ItemViewModel(FolderSettings);
            //await ParentShellPageInstance.FilesystemViewModel.SetWorkingDirectoryAsync(NavParam);
            //await viewmodel.SetWorkingDirectoryAsync(NavParam);
            FolderSettings.LayoutModeChangeRequested -= FolderSettings_LayoutModeChangeRequested;
            FolderSettings.LayoutModeChangeRequested += FolderSettings_LayoutModeChangeRequested;
            columnparent = ParentShellPageInstance;
            parameters = (NavigationArguments)eventArgs.Parameter;
            if (parameters.IsLayoutSwitch)
            {
                ReloadItemIcons();
            }
        }

        protected override void InitializeCommandsViewModel()
        {
            CommandsViewModel = new BaseLayoutCommandsViewModel(new BaseLayoutCommandImplementationModel(ParentShellPageInstance, ItemManipulationModel));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            FolderSettings.LayoutModeChangeRequested -= FolderSettings_LayoutModeChangeRequested;
        }

        private async void ReloadItemIcons()
        {
            ParentShellPageInstance.FilesystemViewModel.CancelExtendedPropertiesLoading();
            foreach (ListedItem listedItem in ParentShellPageInstance.FilesystemViewModel.FilesAndFolders.ToList())
            {
                listedItem.ItemPropertiesInitialized = false;
                if (FileList.ContainerFromItem(listedItem) != null)
                {
                    await ParentShellPageInstance.FilesystemViewModel.LoadExtendedItemProperties(listedItem, 24);
                }
            }
        }

        private void FolderSettings_LayoutModeChangeRequested(object sender, LayoutModeEventArgs e)
        {
        }

        override public void StartRenameItem()
        {
            RenamingItem = FileList.SelectedItem as ListedItem;
            if (RenamingItem == null)
            {
                return;
            }
            int extensionLength = RenamingItem.FileExtension?.Length ?? 0;
            ListViewItem listViewItem = FileList.ContainerFromItem(RenamingItem) as ListViewItem;
            TextBox textBox = null;
            if (listViewItem == null)
            {
                return;
            }
            RenamingTextBlock = listViewItem.FindDescendant("ItemName") as TextBlock;
            textBox = listViewItem.FindDescendant("ListViewTextBoxItemName") as TextBox;
            //textBlock = (listViewItem.ContentTemplateRoot as Border).FindDescendant("ItemName") as TextBlock;
            //textBox = (listViewItem.ContentTemplateRoot as Border).FindDescendant("ListViewTextBoxItemName") as TextBox;
            textBox.Text = RenamingTextBlock.Text;
            OldItemName = RenamingTextBlock.Text;
            RenamingTextBlock.Visibility = Visibility.Collapsed;
            textBox.Visibility = Visibility.Visible;
            textBox.Focus(FocusState.Pointer);
            textBox.LostFocus += RenameTextBox_LostFocus;
            textBox.KeyDown += RenameTextBox_KeyDown;

            int selectedTextLength = SelectedItem.ItemName.Length;
            if (!SelectedItem.IsShortcutItem && App.AppSettings.ShowFileExtensions)
            {
                selectedTextLength -= extensionLength;
            }
            textBox.Select(0, selectedTextLength);
            IsRenamingItem = true;
        }

        private void RenameTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                TextBox textBox = sender as TextBox;
                textBox.LostFocus -= RenameTextBox_LostFocus;
                textBox.Text = OldItemName;
                EndRename(textBox);
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Enter)
            {
                TextBox textBox = sender as TextBox;
                textBox.LostFocus -= RenameTextBox_LostFocus;
                CommitRename(textBox);
                e.Handled = true;
            }
        }

        private void RenameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // This check allows the user to use the text box context menu without ending the rename
            if (!(FocusManager.GetFocusedElement() is AppBarButton or Popup))
            {
                TextBox textBox = e.OriginalSource as TextBox;
                CommitRename(textBox);
            }
        }

        private async void CommitRename(TextBox textBox)
        {
            EndRename(textBox);
            string newItemName = textBox.Text.Trim().TrimEnd('.');

            bool successful = await UIFilesystemHelpers.RenameFileItemAsync(RenamingItem, OldItemName, newItemName, ParentShellPageInstance);
            if (!successful)
            {
                RenamingItem.ItemName = OldItemName;
            }
        }

        private void EndRename(TextBox textBox)
        {
            if (textBox == null || textBox.Parent == null)
            {
                // Navigating away, do nothing
            }
            else
            {
                textBox.Visibility = Visibility.Collapsed;
                RenamingTextBlock.Visibility = Visibility.Visible;
            }

            textBox.LostFocus -= RenameTextBox_LostFocus;
            textBox.KeyDown -= RenameTextBox_KeyDown;
            FileNameTeachingTip.IsOpen = false;
            IsRenamingItem = false;
        }

        public override void ResetItemOpacity()
        {
            // throw new NotImplementedException();
        }

        protected override ListedItem GetItemFromElement(object element)
        {
            if (element is ListViewItem item)
            {
                return (item.DataContext as ListedItem) ?? (item.Content as ListedItem) ?? (FileList.ItemFromContainer(item) as ListedItem);
            }
            return null;
        }

        #region IDisposable

        public override void Dispose()
        {
            UnhookEvents();
            CommandsViewModel?.Dispose();
        }

        #endregion IDisposable

        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null)
            {
                // Do not commit rename if SelectionChanged is due to selction rectangle (#3660)
                //FileList.CommitEdit();
            }
            SelectedItems = FileList.SelectedItems.Cast<ListedItem>().Where(x => x != null).ToList();
        }

        private void FileList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (!IsRenamingItem)
            {
                HandleRightClick(sender, e);
            }
        }

        private void HandleRightClick(object sender, RightTappedRoutedEventArgs e)
        {
            var objectPressed = ((FrameworkElement)e.OriginalSource).DataContext as ListedItem;
            if (objectPressed != null)
            {
                {
                    return;
                }
            }
            // Check if RightTapped row is currently selected
            if (IsItemSelected)
            {
                if (SelectedItems.Contains(objectPressed))
                {
                    return;
                }
            }

            // The following code is only reachable when a user RightTapped an unselected row
            ItemManipulationModel.SetSelectedItem(objectPressed);
        }

        private async void FileList_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var ctrlPressed = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            var shiftPressed = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);

            if (e.Key == VirtualKey.Enter && !e.KeyStatus.IsMenuKeyDown)
            {
                if (!IsRenamingItem)
                {
                    NavigationHelpers.OpenSelectedItems(ParentShellPageInstance, false);
                    e.Handled = true;
                }
            }
            else if (e.Key == VirtualKey.Enter && e.KeyStatus.IsMenuKeyDown)
            {
                FilePropertiesHelpers.ShowProperties(ParentShellPageInstance);
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Space)
            {
                if (!IsRenamingItem && !ParentShellPageInstance.NavToolbarViewModel.IsEditModeEnabled)
                {
                    e.Handled = true;
                    if (App.MainViewModel.IsQuickLookEnabled)
                    {
                        await QuickLookHelpers.ToggleQuickLook(ParentShellPageInstance);
                    }
                }
            }
            else if (e.KeyStatus.IsMenuKeyDown && (e.Key == VirtualKey.Left || e.Key == VirtualKey.Right || e.Key == VirtualKey.Up))
            {
                // Unfocus the GridView so keyboard shortcut can be handled
                NavToolbar?.Focus(FocusState.Pointer);
            }
            else if (ctrlPressed && shiftPressed && (e.Key == VirtualKey.Left || e.Key == VirtualKey.Right || e.Key == VirtualKey.W))
            {
                // Unfocus the ListView so keyboard shortcut can be handled (ctrl + shift + W/"->"/"<-")
                NavToolbar?.Focus(FocusState.Pointer);
            }
            else if (e.KeyStatus.IsMenuKeyDown && shiftPressed && e.Key == VirtualKey.Add)
            {
                // Unfocus the ListView so keyboard shortcut can be handled (alt + shift + "+")
                NavToolbar?.Focus(FocusState.Pointer);
            }
        }

        private void FileList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext is ListedItem && !AppSettings.OpenItemsWithOneclick)
            {
                if (listViewItem != null)
                {
                    //_ = VisualStateManager.GoToState(listViewItem, "CurrentItem", true);
                }
                var item = (e.OriginalSource as FrameworkElement).DataContext as ListedItem;
                if (item.PrimaryItemAttribute == Windows.Storage.StorageItemTypes.Folder)
                {
                    //var pane = new ModernShellPage();
                    //pane.FilesystemViewModel = new ItemViewModel(InstanceViewModel?.FolderSettings);
                    //await pane.FilesystemViewModel.SetWorkingDirectoryAsync(item.ItemPath);
                    //pane.IsPageMainPane = false;
                    //pane.NavParams = item.ItemPath;
                    DismissOtherBlades(sender as ListView);
                    if (item.ContainsFilesOrFolders)
                    {
                        listViewItem = (FileList.ContainerFromItem(item) as ListViewItem);
                        var frame = new Frame();
                        var blade = new BladeItem();
                        blade.Content = frame;
                        ColumnHost.Items.Add(blade);
                        //pane.NavigateWithArguments(typeof(ColumnViewBase), new NavigationArguments()
                        //{
                        //    NavPathParam = item.ItemPath,
                        //    AssociatedTabInstance = ParentShellPageInstance
                        //});

                        frame.Navigate(typeof(ColumnShellPage), new ColumnParam
                        {
                            Column = 1,
                            Path = item.ItemPath
                        });
                    }
                }
                else
                {
                    NavigationHelpers.OpenSelectedItems(ParentShellPageInstance, false);
                }
            }
            ResetRenameDoubleClick();
        }

        private void FileList_Holding(object sender, HoldingRoutedEventArgs e)
        {
            HandleRightClick(sender, e);
        }

        private void HandleRightClick(object sender, HoldingRoutedEventArgs e)
        {
            var objectPressed = ((FrameworkElement)e.OriginalSource).DataContext as ListedItem;
            if (objectPressed != null)
            {
                {
                    return;
                }
            }
            // Check if RightTapped row is currently selected
            if (IsItemSelected)
            {
                if (SelectedItems.Contains(objectPressed))
                {
                    return;
                }
            }

            // The following code is only reachable when a user RightTapped an unselected row
            ItemManipulationModel.SetSelectedItem(objectPressed);
        }

        private void DismissOtherBlades(ListView listView)
        {
            var blade = listView.FindAscendant<BladeItem>();
            var index = ColumnHost.ActiveBlades.IndexOf(blade);
            if (index == 0)
            {
                try
                {
                    while (ColumnHost.ActiveBlades.Count > 1)
                    {
                        ColumnHost.Items.RemoveAt(1);
                        ColumnHost.ActiveBlades.RemoveAt(1);
                    }
                }
                catch
                {
                }
            }
            else
            {
                try
                {
                    while (ColumnHost.ActiveBlades.Count > index + 1)
                    {
                        ColumnHost.Items.RemoveAt(index + 1);
                        ColumnHost.ActiveBlades.RemoveAt(index + 1);
                    }
                }
                catch
                {
                }
            }
        }

        private async void FileList_ItemTapped(object sender, TappedRoutedEventArgs e)
        {
            if (listViewItem != null)
            {
                //_ = VisualStateManager.GoToState(listViewItem, "CurrentItem", true);
            }
            var ctrlPressed = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            var shiftPressed = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);

            if (ctrlPressed || shiftPressed) // Allow for Ctrl+Shift selection
            {
                return;
            }
            // Check if the setting to open items with a single click is turned on
            if (AppSettings.OpenItemsWithOneclick)
            {
                ResetRenameDoubleClick();
                await Task.Delay(200); // The delay gives time for the item to be selected
                var item = (e.OriginalSource as FrameworkElement)?.DataContext as ListedItem;
                if (item.PrimaryItemAttribute == Windows.Storage.StorageItemTypes.Folder)
                {
                    //var pane = new ModernShellPage();
                    //pane.FilesystemViewModel = new ItemViewModel(InstanceViewModel?.FolderSettings);
                    //await pane.FilesystemViewModel.SetWorkingDirectoryAsync(item.ItemPath);
                    //pane.IsPageMainPane = false;
                    //pane.NavParams = item.ItemPath;
                    DismissOtherBlades(sender as ListView);
                    if (item.ContainsFilesOrFolders)
                    {
                        listViewItem = (FileList.ContainerFromItem(item) as ListViewItem);
                        var frame = new Frame();
                        var blade = new BladeItem();
                        blade.Content = frame;
                        ColumnHost.Items.Add(blade);
                        //pane.NavigateWithArguments(typeof(ColumnViewBase), new NavigationArguments()
                        //{
                        //    NavPathParam = item.ItemPath,
                        //    AssociatedTabInstance = ParentShellPageInstance
                        //});
                        
                        frame.Navigate(typeof(ColumnShellPage), new ColumnParam
                        {
                            Column = 1,
                            Path = item.ItemPath
                        });
                    }
                }
                else
                {
                    NavigationHelpers.OpenSelectedItems(ParentShellPageInstance, false);
                }
            }
            else
            {
                var clickedItem = e.OriginalSource as FrameworkElement;
                if (clickedItem is TextBlock && ((TextBlock)clickedItem).Name == "ItemName")
                {
                    CheckRenameDoubleClick(clickedItem?.DataContext);
                }
                else if (IsRenamingItem)
                {
                    ListViewItem listViewItem = FileList.ContainerFromItem(RenamingItem) as ListViewItem;
                    if (listViewItem != null)
                    {
                        var textBox = listViewItem.FindDescendant("ListViewTextBoxItemName") as TextBox;
                        EndRename(textBox);
                    }
                }
            }
        }

        private void ColumnShellPage_NotifyRoot(object sender, EventArgs e)
        {
            var column = sender as ColumnParam;
            try
            {
                while (ColumnHost.ActiveBlades.Count > column.Column)
                {
                    ColumnHost.ActiveBlades.RemoveAt(column.Column + 1);
                }
            }
            catch
            {
            }
            var frame = new Frame();
            var blade = new BladeItem();
            blade.Content = frame;
            ColumnHost.Items.Add(blade);
            //pane.NavigateWithArguments(typeof(ColumnViewBase), new NavigationArguments()
            //{
            //    NavPathParam = item.ItemPath,
            //    AssociatedTabInstance = ParentShellPageInstance
            //});

            frame.Navigate(typeof(ColumnShellPage), new ColumnParam
            {
                Column = ColumnHost.ActiveBlades.IndexOf(blade),
                Path = column.Path
            });
        }

        private void StackPanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var parentContainer = DependencyObjectHelpers.FindParent<ListViewItem>(e.OriginalSource as DependencyObject);
            if (parentContainer.IsSelected)
            {
                return;
            }
            // The following code is only reachable when a user RightTapped an unselected row
            ItemManipulationModel.SetSelectedItem(FileList.ItemFromContainer(parentContainer) as ListedItem);
        }

        private void FileListListItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.KeyModifiers == VirtualKeyModifiers.Control)
            {
                if ((sender as SelectorItem).IsSelected)
                {
                    (sender as SelectorItem).IsSelected = false;
                    // Prevent issues arising caused by the default handlers attempting to select the item that has just been deselected by ctrl + click
                    e.Handled = true;
                }
            }
            else if (e.GetCurrentPoint(sender as UIElement).Properties.IsLeftButtonPressed)
            {
                if (!(sender as SelectorItem).IsSelected)
                {
                    (sender as SelectorItem).IsSelected = true;
                }
            }
            else if (e.GetCurrentPoint(sender as UIElement).Properties.IsMiddleButtonPressed)
            {
                if (!(sender as SelectorItem).IsSelected)
                {
                    (sender as SelectorItem).IsSelected = true;
                }
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var itemContainer = (sender as Grid)?.FindAscendant<ListViewItem>();
            if (itemContainer is null)
            {
                return;
            }

            itemContainer.ContextFlyout = ItemContextMenuFlyout;
        }

        private void FileList_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (!args.InRecycleQueue)
            {
                InitializeDrag(args.ItemContainer);
                args.ItemContainer.PointerPressed -= FileListListItem_PointerPressed;
                args.ItemContainer.PointerPressed += FileListListItem_PointerPressed;

                if (args.Item is ListedItem item && !item.ItemPropertiesInitialized)
                {
                    args.RegisterUpdateCallback(3, async (s, c) =>
                    {
                        await ParentShellPageInstance.FilesystemViewModel.LoadExtendedItemProperties(item, 24);
                    });
                }
            }
            else
            {
                UninitializeDrag(args.ItemContainer);
                args.ItemContainer.PointerPressed -= FileListListItem_PointerPressed;
                if (args.Item is ListedItem item)
                {
                    ParentShellPageInstance.FilesystemViewModel.CancelExtendedPropertiesLoadingForItem(item);
                }
            }
        }

        public IBaseLayout LastColumnBrowser => IsLastColumnBase ? this : ((ColumnHost.ActiveBlades.Last().Content as Frame).Content as ColumnShellPage).SlimContentPage as ColumnViewBase;

        public IShellPage LastColumnShellPage => IsLastColumnBase ? ParentShellPageInstance : ((ColumnHost.ActiveBlades.Last().Content as Frame).Content as ColumnShellPage);

        public bool IsLastColumnBase => ColumnHost.ActiveBlades.Count == 1;
    }
}
