using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Woodpecker.Storage;
using Woodpecker.Game.Items;
using Woodpecker.Game.Items.Pets;

namespace Woodpecker.Tools
{
    public partial class publicRoomItemMainForm : Form
    {
        public publicRoomItemMainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String entered = "";
            foreach (String line in textBox1.Lines)
            {
                String[] value = line.Split(' ');
                if (!entered.Contains("," + value[1] + ","))
                {
                    entered += "," + value[1] + ",";
                    //a1611 sun_chair 16 11 0 2 2
                    Boolean chair = false;
                    if (value[6] == "2") chair = true;

                    itemDefinition editedDefinition = new itemDefinition();
                    editedDefinition.directoryID = 0;
                    editedDefinition.Sprite = value[1];
                    editedDefinition.Color = "";
                    editedDefinition.Length = (byte)1.00F;
                    editedDefinition.Width = (byte)1.00F;
                    editedDefinition.topHeight = (float)(chair ? 1.00F : 0.00F);

                    itemDefinition.itemBehaviourContainer newBehaviourContainer = new itemDefinition.itemBehaviourContainer();
                    newBehaviourContainer.isPublicSpaceObject = true;
                    newBehaviourContainer.canSitOnTop = chair;
                    editedDefinition.Behaviour = newBehaviourContainer;

                    this.saveItemDefinition(editedDefinition);
                }
            }
            Engine.Game.Items.loadDefinitions();
        }

        private void saveItemDefinition(itemDefinition pDefinition)
        {
            Database dbClient = new Database(false, false);
            dbClient.addParameterWithValue("directoryid", pDefinition.directoryID);
            dbClient.addParameterWithValue("sprite", pDefinition.Sprite);
            dbClient.addParameterWithValue("color", pDefinition.Color);
            dbClient.addParameterWithValue("length", pDefinition.Length);
            dbClient.addParameterWithValue("width", pDefinition.Width);
            dbClient.addParameterWithValue("topheight", pDefinition.topHeight);
            dbClient.addParameterWithValue("behaviour", pDefinition.Behaviour.ToString());

            dbClient.Open();
            if (dbClient.Ready)
            {
                dbClient.runQuery("DELETE FROM items_definitions WHERE sprite = @sprite"); // Drop old definition
                dbClient.runQuery( // Insert new/edited definition
                    "INSERT INTO items_definitions " +
                    "VALUES " +
                    "(null,@directoryid,@sprite,@color,@length,@width,@topheight,@behaviour)");
                dbClient.Close();
            }
        }
        private void saveItemInstance(String defid, String roomId, String x, String y, String z, String rot, String customData)
        {
            try
            {
                Database dbClient = new Database(false, false);
                dbClient.addParameterWithValue("definitionid", defid);
                dbClient.addParameterWithValue("roomid", roomId);
                dbClient.addParameterWithValue("x", x);
                dbClient.addParameterWithValue("y", y);
                dbClient.addParameterWithValue("z", z);
                dbClient.addParameterWithValue("rot", rot);
                dbClient.addParameterWithValue("customdata", customData);

                dbClient.Open();
                if (dbClient.Ready)
                {
                    dbClient.runQuery("DELETE FROM items WHERE customdata = @customdata AND roomid = @roomid"); // Drop old definition
                    dbClient.runQuery( // Insert new/edited definition
                        "INSERT INTO items " +
                        "VALUES " +
                        "(null,@definitionid,'0',@roomid,@x,@y,@z,@rot,@customdata,'0',null,null)");
                    dbClient.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //a1611 sun_chair 16 11 0 2 2
            foreach (String line in textBox1.Lines)
            {
                String[] value = line.Split(' ');
                String id = Engine.Game.Items.getItemDefinitionByName(value[1]).ID.ToString();
                saveItemInstance(id, textBox2.Text, value[2], value[3], value[4], value[5], value[0]);
            }
        }
    }
}
