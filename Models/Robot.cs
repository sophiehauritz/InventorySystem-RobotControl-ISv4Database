using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace InventorySystem.Models
{
    public class Robot
    {
        // Connection
        public const int UrsPort       = 30002; // URScript
        public const int DashboardPort = 29999; // Dashboard
        public string IpAddress { get; set; } = "127.0.0.1";

        // Grid calibration (all meters unless marked mm)
        public double Pitch   { get; set; } = 0.10;  // spacing between A/B/C
        public double RowY    { get; set; } = 0.10;  // Y of the bin row
        public double Sx      { get; set; } = 0.30;  // shipment box X
        public double Sy      { get; set; } = 0.30;  // shipment box Y

        public double ZAbove  { get; set; } = 0.070; // safe height
        public double ZDown   { get; set; } = 0.015; // touch height

        public double Acc     { get; set; } = 1.0;
        public double Vel     { get; set; } = 0.20;
        public double Blend   { get; set; } = 0.002;

        public int    SignX   { get; set; } = 1;     // flip if axes are reversed
        public int    SignY   { get; set; } = 1;

        public double OffsetXmm { get; set; } = 0;   // fine nudge (mm)
        public double OffsetYmm { get; set; } = 0;   // fine nudge (mm)

        // Public API used by your ViewModel

        public void SendUrscript(string urs)
        {
            SendString(DashboardPort, "brake release\n");
            SendString(UrsPort, EnsureNl(urs));
        }

        /// <summary>
        /// itemId: 1=A, 2=B, 3=C
        /// </summary>
        public string BuildPickToShipmentScript(int itemId)
        {
            var ic = CultureInfo.InvariantCulture;

            double baseX = (itemId - 1) * Pitch * SignX; // 0, ±0.10, ±0.20
            double baseY = RowY * SignY;

            double offX = OffsetXmm / 1000.0;
            double offY = OffsetYmm / 1000.0;

            double itemX = baseX + offX;
            double itemY = baseY + offY;

            double sX = (Sx * SignX) + offX;
            double sY = (Sy * SignY) + offY;

            string script =
$@"def move_item_to_shipment_box():
  a = {Acc.ToString("0.###", ic)}
  v = {Vel.ToString("0.###", ic)}
  r = {Blend.ToString("0.###", ic)}
  z_above = {ZAbove.ToString("0.###", ic)}
  z_down  = {ZDown.ToString("0.###", ic)}

  P0 = get_actual_tcp_pose()

  def at_xy(x, y):
    return p[P0[0] + x, P0[1] + y, P0[2], P0[3], P0[4], P0[5]]
  end

  def go_to_xy(x, y):
    tgt = at_xy(x, y)
    pre = pose_trans(tgt, p[0,0, z_above + 0.050, 0,0,0])
    movej(pre, a, v)
    movel(pose_trans(tgt, p[0,0, z_above, 0,0,0]), a, v, r)
  end

  def go_down_up_at_xy(x, y):
    tgt = at_xy(x, y)
    movel(pose_trans(tgt, p[0,0, z_down, 0,0,0]), a, v, r)
    sleep(0.10)
    movel(pose_trans(tgt, p[0,0, z_above, 0,0,0]), a, v, r)
  end

  textmsg(""ITEM_X={itemX.ToString("0.000", ic)}; ITEM_Y={itemY.ToString("0.000", ic)}"")
  textmsg(""SBOX_X={sX.ToString("0.000", ic)}; SBOX_Y={sY.ToString("0.000", ic)}"")

  # Pick
  go_to_xy({itemX.ToString("0.000", ic)}, {itemY.ToString("0.000", ic)})
  go_down_up_at_xy({itemX.ToString("0.000", ic)}, {itemY.ToString("0.000", ic)})

  # Place
  go_to_xy({sX.ToString("0.000", ic)}, {sY.ToString("0.000", ic)})
  go_down_up_at_xy({sX.ToString("0.000", ic)}, {sY.ToString("0.000", ic)})
end

move_item_to_shipment_box()
";
            return script;
        }

        // Internals
        private void SendString(int port, string message)
        {
            using var client = new TcpClient(IpAddress, port);
            using var stream = client.GetStream();
            var bytes = Encoding.ASCII.GetBytes(message);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static string EnsureNl(string s) => s.EndsWith("\n") ? s : s + "\n";
    }
}

