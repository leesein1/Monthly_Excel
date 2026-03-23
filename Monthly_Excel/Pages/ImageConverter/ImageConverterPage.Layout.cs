using System.Windows.Forms;

namespace Monthly_Excel.Pages.ImageConverter
{
    public partial class ImageConverterPage
    {
        private void UpdateResponsiveLayout()
        {
            ApplyMiddleLayout(ClientSize.Width);
        }

        private void ApplyMiddleLayout(int width)
        {
            var targetOrientation = width < 900 ? Orientation.Horizontal : Orientation.Vertical;
            if (_middleSplit.Orientation != targetOrientation)
            {
                ResetSplitterForOrientationChange(targetOrientation);
                _middleSplit.Orientation = targetOrientation;
            }

            AdjustSplitterLayout();
        }

        private void ResetSplitterForOrientationChange(Orientation targetOrientation)
        {
            _middleSplit.Panel1MinSize = 0;
            _middleSplit.Panel2MinSize = 0;

            var currentPrimary = _middleSplit.Orientation == Orientation.Vertical
                ? _middleSplit.ClientSize.Width
                : _middleSplit.ClientSize.Height;
            var targetPrimary = targetOrientation == Orientation.Vertical
                ? _middleSplit.ClientSize.Width
                : _middleSplit.ClientSize.Height;
            var maxDistance = System.Math.Max(0, System.Math.Min(currentPrimary, targetPrimary) - _middleSplit.SplitterWidth);

            _middleSplit.SplitterDistance = System.Math.Min(_middleSplit.SplitterDistance, maxDistance);
        }

        private void AdjustSplitterLayout()
        {
            var availablePrimary = _middleSplit.Orientation == Orientation.Vertical
                ? _middleSplit.ClientSize.Width
                : _middleSplit.ClientSize.Height;
            if (availablePrimary <= 0)
            {
                return;
            }

            var desiredPanel1MinSize = _middleSplit.Orientation == Orientation.Vertical ? 240 : 180;
            var desiredPanel2MinSize = _middleSplit.Orientation == Orientation.Vertical ? 320 : 220;
            var minimumRequiredSize = desiredPanel1MinSize + desiredPanel2MinSize + _middleSplit.SplitterWidth;

            if (availablePrimary < minimumRequiredSize)
            {
                _middleSplit.Panel1MinSize = 0;
                _middleSplit.Panel2MinSize = 0;
                return;
            }

            _middleSplit.Panel1MinSize = desiredPanel1MinSize;
            _middleSplit.Panel2MinSize = desiredPanel2MinSize;

            var minimumDistance = _middleSplit.Panel1MinSize;
            var maximumDistance = availablePrimary - _middleSplit.Panel2MinSize - _middleSplit.SplitterWidth;
            if (maximumDistance < minimumDistance)
            {
                _middleSplit.Panel1MinSize = 0;
                _middleSplit.Panel2MinSize = 0;
                return;
            }

            var preferredDistance = _middleSplit.Orientation == Orientation.Vertical
                ? System.Math.Max(320, availablePrimary / 3)
                : System.Math.Max(220, availablePrimary / 2);
            var currentDistance = _middleSplit.SplitterDistance;
            var desiredDistance = currentDistance < minimumDistance || currentDistance > maximumDistance
                ? preferredDistance
                : currentDistance;

            _middleSplit.SplitterDistance = System.Math.Min(System.Math.Max(desiredDistance, minimumDistance), maximumDistance);
        }
    }
}
