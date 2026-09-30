using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(110)]
    internal sealed class ShowAssetPreviewDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var property = element.Property;

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                var msg = $"Ошибка: [ShowAssetPreview] работает только со ссылками на ассеты! (Поле: {element.Name})";
                return new HelpBox().Setup(msg, HelpBoxMessageType.Error);
            }

            var attr = element.Member.GetCustomAttribute<ShowAssetPreviewAttribute>(true);
            var container = new VisualElement()
                .SetFlex(FlexDirection.Row);

            var previewImage = new Image()
                .SetWidth(attr.Width)
                .SetHeight(attr.Height)
                .SetMargin(left: 134f, top: 2f, bottom: 4f);
            container.Add(previewImage);

            void UpdatePreview()
            {
                var targetAsset = property.objectReferenceValue;
                if (!targetAsset)
                {
                    previewImage.image = null;
                    previewImage.SetDisplayStyle(DisplayStyle.None);
                    return;
                }

                previewImage.SetDisplayStyle(DisplayStyle.Flex);
                var previewTexture = AssetPreview.GetAssetPreview(targetAsset);

                if (!previewTexture && AssetPreview.IsLoadingAssetPreview(targetAsset.GetInstanceID()))
                {
                    previewImage.schedule.Execute(() =>
                    {
                        var loadingTexture = AssetPreview.GetAssetPreview(targetAsset);
                        if (loadingTexture != null)
                        {
                            previewImage.image = loadingTexture;
                        }
                    })
                    .Until(() => AssetPreview.GetAssetPreview(targetAsset) != null || property.objectReferenceValue != targetAsset);
                }
                else
                {
                    previewImage.image = previewTexture
                        ? previewTexture
                        : AssetPreview.GetMiniThumbnail(targetAsset);
                }
            }

            UpdatePreview();

            container.TrackPropertyValue(property, _ => UpdatePreview());
            return container;
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            return base.CanDrawInternal(element) &&
                   element.Property != null &&
                   element.Member.IsDefined(typeof(ShowAssetPreviewAttribute), true);
        }

        public override bool IsDecorator => true;
    }
}