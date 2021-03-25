using GameClass.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameClientCW.View
{
    /// <summary>
    /// Interaction logic for AddParamWindow.xaml
    /// </summary>
    public partial class AddParamWindow : Window
    {
        Item item = new Item();
        Mod mapmod = new Mod(); 
        public AddParamWindow(Item itm)
        {
            item = itm;
            InitializeComponent();  
            foreach (ItemParamsName param in Enum.GetValues(typeof(ItemParamsName)))
            {
                bool iseq = false;
                foreach (Params p in item.Parametrs)
                {
                    if (p.iParam == param) 
                    {
                        iseq = true;
                        break;
                    }
                }
                if (!iseq)
                {
                    cmb.Items.Add(param);
                }
            }
        }
        public AddParamWindow(Mod mmod)
        {
            mapmod = mmod;
            InitializeComponent();
            foreach (ModParamsName param in Enum.GetValues(typeof(ModParamsName)))
            {
                bool iseq = false;
                foreach (Params mod in mmod.Parametrs)
                    {
                    if (mod.mParam == param)
                    {
                        iseq = true;
                        break;
                    }
                }
                if (!iseq)
                {
                    cmb.Items.Add(param);
                }
            }
 
        }
        public TCPQuery<Mod> tCPQueryM
        {
            get; set;
        }
        public TCPQuery<Item> tCPQueryI
        {
            get; set;
        }


        private void Regiser_Click(object sender, RoutedEventArgs e)
        {
            if (item.Parametrs is null && cmb.Items.Count>0) 
            {
                Params pr = new Params();
                pr.ID = -1;
                pr.mParam = (ModParamsName)cmb.Items[cmb.SelectedIndex];
                pr.value = Value.Text;
                mapmod.Parametrs.Add(pr); 
                tCPQueryM = new TCPQuery<Mod>("127.0.0.1", mapmod);
                
                formDE(false);
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    bool normalExt = tCPQueryM.send((int)Ports.UpdateMod);
                    this.Dispatcher.Invoke(() => {
                        if (normalExt)
                        {
                            RegFin(tCPQueryM.objectTGS);
                        }
                        else
                        {
                            Error.Text = "Не удалось подключится к серверу, повторите попытку позднее";
                            formDE(true);
                        }
                    });

                }); 
            }else if (cmb.Items.Count > 0)
            {
                Params pr = new Params();
                pr.ID = -1;
                pr.iParam = (ItemParamsName)cmb.Items[cmb.SelectedIndex];
                pr.value = Value.Text;
                item.Parametrs.Add(pr);
                tCPQueryI = new TCPQuery<Item>("127.0.0.1", item);

                formDE(false);
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    bool normalExt = tCPQueryI.send((int)Ports.UpdateItem);
                    this.Dispatcher.Invoke(() => {
                        if (normalExt)
                        {
                            RegFin(tCPQueryI.objectTGS);
                        }
                        else
                        {
                            Error.Text = "Не удалось подключится к серверу, повторите попытку позднее";
                            formDE(true);
                        }
                    });

                });
            }

        }
        private void RegFin(Mod md)
        {
            switch (md.ID)
            {
                case -1:
                    Error.Text = "Не удалось обновить модификатор, повторите попытку позднее";
                    formDE(true);
                    break;
                default:
                    Close();
                    break;

            }
        }
        private void RegFin(Item map)
        {
            switch (item.ID)
            {
                case -1:
                    Error.Text = "Не удалось обновить предмет, повторите попытку позднее";
                    formDE(true);
                    break;
                default:
                    Close();
                    break;

            }
        }
        private void formDE(bool isTF)
        {
            if (isTF)
            {
                load.Visibility = Visibility.Hidden;
            }
            else
            {
                load.Visibility = Visibility.Visible;
            }
            register.IsEnabled = isTF;
            cmb.IsEnabled = isTF;
            Value.IsEnabled = isTF;
            Auf.IsEnabled = isTF;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = true;
        }

        private void cmb_TargetUpdated(object sender, DataTransferEventArgs e)
        { 
        }

 
    }
}
