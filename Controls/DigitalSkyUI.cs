using Flurl;
using Flurl.Http;
using GMap.NET;
using GMap.NET.WindowsForms;
using MissionPlanner.ArduPilot.Mavlink;
using MissionPlanner.Maps;
using MissionPlanner.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace MissionPlanner.Controls
{
    public partial class DigitalSkyUI : UserControl, IActivate
    {
        GMapOverlay _markeroverlay;
        private DigitalSky digitalSky;

        public DigitalSkyUI()
        {
            InitializeComponent();
        }

        public async void test()
        {
            // Disabled: no external connections in offline-hardened build
            return;
        }

        public void Activate()
        {
            myGMAP1.MapProvider = GCSViews.FlightData.mymap.MapProvider;
            myGMAP1.MaxZoom = 24;
            myGMAP1.Zoom = 5;
            myGMAP1.DisableFocusOnMouseEnter = true;
            myGMAP1.DragButton = MouseButtons.Left;
            myGMAP1.Position = new PointLatLng(17.8758086, 77.7369485);
            myGMAP1.FillEmptyTiles = true;

            _markeroverlay = new GMapOverlay("markers");
            myGMAP1.Overlays.Add(_markeroverlay);

            myGMAP1.Invalidate();

            digitalSky = new DigitalSky();
        }

        private async void But_login_Click(object sender, EventArgs e)
        {
            // Disabled: no external connections in offline-hardened build
            CustomMessageBox.Show("Digital Sky is disabled in this build.");
            return;
        }

        private async void Cmb_drones_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Disabled: no external connections in offline-hardened build
            return;
        }

        private void Cmb_applications_SelectedIndexChanged(object sender, EventArgs e)
        {
            var perm = (JToken)cmb_applications.SelectedValue;

            var flyArea = perm["flyArea"].Children().Select(a =>
                new PointLatLngAlt(a["latitude"].Value<double>(), a["longitude"].Value<double>(), perm["maxAltitude"].Value<double>() / 3.281, perm["id"].Value<string>()));

            lbl_approvedstatus.Text = perm["status"].Value<string>();

            _markeroverlay.Markers.Clear();
            _markeroverlay.Polygons.Clear();

            _markeroverlay.Polygons.Add(new GMapPolygon(flyArea.ToList().Select(a => (PointLatLng)a).ToList(), ""));

            foreach (var pointLatLngAlt in flyArea)
            {
                _markeroverlay.Markers.Add(new GMapMarkerWP(pointLatLngAlt, ""));
            }

            var rect = myGMAP1.GetRectOfAllMarkers(null);
            if (rect.HasValue)
            {
                // 10% padding
                rect.Value.Inflate(rect.Value.HeightLat * 0.1, rect.Value.WidthLng * 0.1);
                myGMAP1.SetZoomToFitRect(rect.Value);
            }
        }

        private async void But_dlartifact_Click(object sender, EventArgs e)
        {
            // Disabled: no external connections in offline-hardened build
            return;
        }

        private async void But_uploadflightlog_Click(object sender, EventArgs e)
        {
            // Disabled: no external connections in offline-hardened build
            return;
        }
    }
}
