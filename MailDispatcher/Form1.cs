using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailDispatcher.Entity;
using System.Net;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Text.RegularExpressions;


namespace MailDispatcher
{
    
    public partial class Form1 : Form
    {
        int brmails = 0;

        public Form1()
        {
            InitializeComponent();
            lblStart.Text ="Start" +"-"+ DateTime.Now.ToString();
        }
        Int64 i = 0;
        private void tCheckMail_Tick(object sender, EventArgs e)
        {
            String sguid = "2443C5E1-3B7A-4547-89AC-18CC8374D6B3";
            Guid uid = new Guid(sguid);
            label1.Text = "Check:" +DateTime.Now.ToString()+"->"+brmails.ToString();
            try
            {
                i++;

                lblTick.Text = DateTime.Now.ToString() +" - "+ i.ToString();
            //tCheckMail.Stop();
                DateTime today = System.DateTime.Now;

                DateTime answer2 = today.AddDays(-36);
           using (dBTeslaEntities ctx = new dBTeslaEntities())
            {
                IEnumerable<ChangeManagementMaster> pr = ctx.ChangeManagementMaster
                        .Where(c => c.Mail == 1 && c.Status==1)
                        .Where(t => t.Datum >= DateTime.Today)
                        .OrderBy(c => c.Datum).ToList();
               // if (pr.Count() == 0) return;
                foreach (var chmaster in pr)
                {
                    IEnumerable<ChangeManagementDetail> dt = ctx.ChangeManagementDetail.Where(c => c.ID_CMaster == chmaster.ID).
                                                             Where(c => c.Status == 1 && c.Mail ==0).
                                                             Where(c=> c.ID_TeachersCM != uid).
                                                             ToList();
                        //if (dt.Count() == 0)
                        //{
                        //    updateMailMaster(chmaster.ID);
                        //    return;
                        //}
                        //tCheckMail.Stop();
                            // label1.Text = "Stop";
                        var results = dt
                   .OrderByDescending(item => item.ID_TeachersCM)
                   .Where(item=> item.ID_TeachersCM != uid)
                   .GroupBy(item => item.ID_TeachersCM)
                   .Select(grp => grp.First())
                   .OrderBy(item => item.ID)
                   .ToArray();

                if(dt.Count() != results.Count())
                        {
                            //foreach (var chdITEM in results)
                            //{


                            //}

                                foreach (var chdITEM in results)
                            {
                                String s = chdITEM.ID_TeachersCM.ToString();
                                Guid gd = new Guid(s);
                                IEnumerable <ChangeManagementDetail> group = dt.ToList().Where(i => i.ID_TeachersCM == gd & i.ID_CMaster == chdITEM.ID_CMaster).ToList();
                                DateTime date = Convert.ToDateTime(chmaster.Datum);
                                String datum = date.ToString("dd.MM.yyyy");
                                if (group.Count()>1)
                                {
                                    var sadresa = ctx.Teachers.SingleOrDefault(c => c.ID == chdITEM.ID_TeachersCM);
                                    if(sadresa != null)
                                    {
                                        if(emailIsValid(sadresa.Email))
                                        {
                                            var sodstnast = ctx.Teachers.SingleOrDefault(c => c.ID == chmaster.ID_Teachers);
                                            string snametcm = "";
                                            if (sodstnast != null )
                                            {
                                                snametcm = sodstnast.Name;
                                            }
                                            int isend = sendMailGroup(group, sadresa.Email, datum, snametcm, sadresa.Name);
                                        }
                                       
                                    }

                                }
                                else
                                {
                                    int isend = sendMailSolo(group,gd);
                                }
                               
                            }
                        }
                     else
                        {
                        
                                        foreach (var chdetail in dt)
                                        {
                                            CMbyID_Result4 res = new CMbyID_Result4();
                                            IEnumerable<CMbyID_Result4> resp1 = ctx.Database.SqlQuery<CMbyID_Result4>(
                                                    "EXEC CMbyID @ParentGuid",
                                                    new SqlParameter("@ParentGuid", chdetail.ID_CMaster)).ToList();
                                            int i = resp1.Count();
                                            //CMbyID_Result4.Parameters.Clear();
                                            foreach (var chdetaildmail in resp1)
                                            {
                                                if (chdetaildmail.Mail == 0)
                                                {
                                                    //Guid uidtsch = new Guid(chdetaildmail.TeachersIDCM.ToString());
                                                    var sadresa = ctx.Teachers.SingleOrDefault(c => c.ID == chdetaildmail.ID_TeachersCM);
                                                    int isend = sendMail(chdetaildmail, sadresa.Email);
                                                    if (isend == 1)
                                                    {
                                                        Guid uidCMdetail = new Guid(chdetaildmail.IDCMdetail.ToString());
                                                        updateMail(uidCMdetail);
                                                    }
                                          }
                                       }
                                }

                        }

                    }

                   // tCheckMail.Enabled = true;


                }
                //tCheckMail.Start();
               // label1.Text = "Start";
            }
            catch (Exception ex)
            {
                //return 1;
            }

        }

        private int sendMail(CMbyID_Result4 resp, string smailCM)
        {
            try
            {
                String disc = " <p style='width:80%;margin-left:20px;'><b>IZJAVA O ODRICANJU OD ODGOVORNOSTI:</b> Sadržaj ove poruke i eventualno priloženih datoteka " +
                "povjerljiv je i namijenjen samo primateljima navedenima u adresi. Svako neovlašteno korištenje," +
                "objavljivanje, prerada, obrada, reprodukcija, prikazivanje, prenošenje, snimanje ili bilo koji " +
                "drugi oblik neovlaštene uporabe ove elektroničke pošte podliježe kaznenoj i prekršajnoj odgovornosti " +
                "kao i građansko pravnoj zaštiti.</p>" +
                "<p style='width:80%;margin-left:20px;'>Ako ste ovu poruku primili greškom, molimo Vas da o tome odmah obavijestite pošiljatelja i izbrišete " +
                "ovu poruku, njene privitke i kopije. Internet komunikacije su nesigurne i I.tehnička škole Tesla ne " +
                "preuzima nikakvu odgovornost s obzirom na bilo koju moguću netočnost bilo kojeg podatka koji je " +
                "sadržan u ovoj poruci, ako takav podatak nije povezan s registriranim predmetom rada I.tehničke škole Tesla.</p><br>";

             String poruka = "";


                if (resp.Poruka != null)
                {
                    String tekst =" <font color='#FF4500'>Dodatna obavijest:</font><br><br>"+resp.Poruka;
                    poruka = "<td style='width:60%;vertical-align:baseline;padding:15px 15px 15px 15px;background-color:#555555;color:#FAFAFA; -webkit-font-smoothing: antialiased;' rowspan='8'>" + tekst + "</td></tr> ";
                }
                DateTime date = Convert.ToDateTime(resp.Datum);
                String datum = date.ToString("dd.MM.yyyy");
                SmtpClient client = new SmtpClient();
                client.Port = 25;
                client.Host = "smtp.skole.hr";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                String postovani = "<p style='margin-left:10px;'>dodijeljena Vam je zamijena čije detalje možete vidjeti u nastavku.</p>";

                String satnicarka = "<p style='margin-left:20px;font-size: 10px;'>Za sve dodatne informacije možete kontaktirati satničarku u uredovno vrijeme</p>";
                satnicarka = satnicarka + "<p style='margin-left:20px;font-size: 10px;'>A/B turnus svaki dan od 13:30 - 15:00</p>";
                satnicarka = satnicarka + "<p style='margin-left:20px;font-size: 10px;'>B/A turnus svaki dan od 7:30 - 9:00</p>";
                satnicarka = satnicarka + "<p style='margin-left:20px;font-size: 10px;'>na tel 099 48 777 36.</p>";

                client.Credentials = new System.Net.NetworkCredential("tesla.info@skole.hr", "CBUN42ef");
                String Body = "<html><body style='background-color: #d9e6f2;margin-left:-10px;margin-right:0px; padding:0;'>" +
                    "<br><h2 style='margin-left:10px;margin-top:0px;'>I. tehnička škola Tesla</h2><hr>" +
                    "<p style='margin-left:10px;'>Poštovani.</p>" + postovani + "<br>" +
                    "<table style='border: 1px solid black;padding:10px;border-collapse: collapse;font-weight: bold;margin-left:20px;'> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Datum zamjene:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + datum + "</td>"+
                    poruka + "</tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Razred:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.Razred + "</td></tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Odsutni nastavnik:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.Name + "</td></tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Nastavnik u zamjeni:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.ProfCM + "</td></tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Predmet:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.SubjectsEx + " </td></tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Predmet u zamjeni:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.SubjectsCm + "</td></tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Kabinet:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.Kabinet + "</td></tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Sat:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.Short + ". " + resp.Sat + "</td></tr> " +
                    "</table>"+ satnicarka+
                   "<br><br><br><p style='margin-left:20px;font-size: 10px;'>" + disc + "</p></html></body>";
                String sSubject = "Zamjena: " + datum;//msdbozic@gmail.com  filip.matesa@gmail.com; zvonimir.petkovic@gmail.com; tomislavjurisic@net.hrmsdbozic@gmail.com

                MailMessage mm = new MailMessage("tesla.info@skole.hr", smailCM, sSubject, Body  );
                //mm.Bcc.Add("brnardica.bozic@skole.hr");
                //mm.Bcc.Add("ivica.jelavic@skole.hr");
                // mm.Bcc.Add("ivcajelavic@gmail.com");
                //mm.Bcc.Add("zvonimir.petkovic@skole.com");
                //mm.Bcc.Add("tomislav.jurisic5@skole.hr");
                //mm.Bcc.Add("ijelavic4@net.amis.hr");
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                mm.IsBodyHtml = true;

                if (smailCM != null)
                {
                    if (emailIsValid(smailCM))
                    {
                        try
                        {
                            client.Send(mm);
                            label1.Text = smailCM +"-"+ DateTime.Now;
                        }
                        catch (Exception ex)
                        {

                            label1.Text = ex.ToString();
                        }
                        

                           // i=+1;
                        //label1.Text = i.ToString();
                    }

                }
                        
                return 1;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        private int sendMailGroup(IEnumerable<ChangeManagementDetail> group, string smailCM, String dtzmj,String tchex, String tshods)
        {
            try
            {
                String poruka = ""; 
                String disc = " <p style='width:80%;margin-left:20px;'><b>IZJAVA O ODRICANJU OD ODGOVORNOSTI:</b> Sadržaj ove poruke i eventualno priloženih datoteka " +
                "povjerljiv je i namijenjen samo primateljima navedenima u adresi. Svako neovlašteno korištenje," +
                "objavljivanje, prerada, obrada, reprodukcija, prikazivanje, prenošenje, snimanje ili bilo koji " +
                "drugi oblik neovlaštene uporabe ove elektroničke pošte podliježe kaznenoj i prekršajnoj odgovornosti " +
                "kao i građansko pravnoj zaštiti.</p>" +
                "<p style='width:80%;margin-left:20px;'>Ako ste ovu poruku primili greškom, molimo Vas da o tome odmah obavijestite pošiljatelja i izbrišete " +
                "ovu poruku, njene privitke i kopije. Internet komunikacije su nesigurne i I.tehnička škola Tesla ne " +
                "preuzima nikakvu odgovornost s obzirom na bilo koju moguću netočnost bilo kojeg podatka koji je " +
                "sadržan u ovoj poruci, ako takav podatak nije povezan s registriranim predmetom rada I.tehničke škole Tesla.</p><br>";
                String postovani = "<p style='margin-left:10px;'>dodijeljena Vam je zamijena čije detalje možete vidjeti u nastavku.</p>";
                String Header = "<html><body style='background-color: #d9e6f2;margin-left:-10px;margin-right:0px; padding:0;'>" +
                                "<br><h2 style='margin-left:10px;margin-top:0px;'>I. tehnička škola Tesla</h2><hr>" +
                                "<p style='margin-left:10px;'>Poštovani.</p>"+ postovani +"<br>" +
                                 "<p style='margin-left:10px;'>Datum   zamjene:<b>  " + dtzmj + "</b></p>" +
                                 "<p style='margin-left:10px;'>Odsutni nastavnik: <b>" + tchex + "</b></p>" +
                                 "<p style='margin-left:10px;'>Nastavnik u zamjeni: <b>" + tshods + "</b></p>" +
                                "<table style='border: 1px solid black;padding:10px;border-collapse: collapse;font-weight: none;margin-left:10px;'> ";
                String htbl = "<tr  style='border: 1px solid black;text-align:center;padding:10px;background-color:#ccc;color:#336699;'><th>Sat</th><th>Raz.</th><th>Predmet</th><th>Pred. u zamjeni</th><th>Kabinet</th></th></tr>";
                using (dBTeslaEntities ctx = new dBTeslaEntities())
                {
                   // DateTime date = Convert.ToDateTime(resp.Datum);
                    //String datum = "dd.MM.yyyy";
                    SmtpClient client = new SmtpClient();
                    client.Port = 25;
                    client.Host = "smtp.skole.hr";
                    client.EnableSsl = true;
                    client.Timeout = 10000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    System.Net.NetworkCredential myCredential = new System.Net.NetworkCredential("tesla.info@skole.hr", "CBUN42ef");
                    // client.Credentials = new System.Net.NetworkCredential("tesla.info@skole.hr", "CBUN42ef");
                    //client.Credentials = myCredential;
                    String Body = "";

                    Dictionary<int,string> map = new Dictionary<int,string>();
                    List<Guid> list = new List<Guid>();
                    int i = 0;
                    foreach (var chdetail in group)
                    {
                        
                        IEnumerable<CMDeatailbyID_Result> resp = ctx.Database.SqlQuery<CMDeatailbyID_Result>(
                                   "EXEC CMDeatailbyID @ParentGuid",
                                   new SqlParameter("@ParentGuid", chdetail.ID)).ToList();
                         i = resp.Count();

                        if (i >0)
                        {

                            foreach (var grdetail in resp)
                            {
                                if (grdetail.Poruka != null)
                                {
                                    // String tekst = " <font color='#FF4500'>Dodatna obavijest:</font><br><br>" + grdetail.Poruka;
                                    poruka = "<td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + grdetail.Poruka + "</td>";
                                    htbl = "<tr  style='border: 1px solid black;text-align:center;padding:10px;background-color:#ccc;color:#336699;'><th>Sat</th><th>Raz.</th><th>Predmet</th><th>Pred. u zamjeni</th><th>Kabinet</th><th>Obavijest</th></tr>";
                                }
                                String Row = "<tr>" +
                                "<td style='border: 1px solid #CDF5B3;padding:5px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + grdetail.Short + ".sat:" + grdetail.Sat + "</td>" +
                                "<td style='border: 1px solid #CDF5B3;padding:5px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + grdetail.Razred + "</td> " +
                                "<td style='border: 1px solid #CDF5B3;padding:5px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + grdetail.SubjectsEx + " </td> " +
                                "<td style='border: 1px solid #CDF5B3;padding:5px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + grdetail.SubjectsCm + "</td>" +
                                "<td style='border: 1px solid #CDF5B3;padding:5px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + grdetail.Kabinet + "</td> " +
                                poruka + "<tr>";
                                map[Convert.ToInt32(grdetail.Short)] = Row;
                                Guid uid = new Guid(grdetail.IDCMdetail.ToString());
                                list.Add(uid);
                            }

                        }
                    }

                    if (i > 0)
                    {
                        map = map.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                        foreach (var item in map)
                        {
                            Body += item.Value;
                        }

                        String satnicarka = "<p style='margin-left:20px;font-size: 10px;'>Za sve dodatne informacije možete kontaktirati satničarku u uredovno vrijeme</p>";
                        satnicarka = satnicarka + "<p style='margin-left:20px;font-size: 10px;'>A/B turnus svaki dan od 13:30 - 15:00</p>";
                        satnicarka = satnicarka + "<p style='margin-left:20px;font-size: 10px;'>B/A turnus svaki dan od 7:30 - 9:00</p>";
                        satnicarka = satnicarka + "<p style='margin-left:20px;font-size: 10px;'>na tel 099 487 77 36.</p>";


                        Body = Header + htbl + Body + "</table><br>" + satnicarka + "<br><p style='margin-left:20px;font-size: 10px;'>" + disc + "</p></body></html>";
                        String sSubject = "Zamjena: " + dtzmj;//msdbozic@gmail.com  filip.matesa@gmail.com; zvonimir.petkovic@gmail.com; tomislavjurisic@net.hrmsdbozic@gmail.com
                        MailMessage mm = new MailMessage("tesla.info@skole.hr", smailCM, sSubject, Body);
                       // mm.Bcc.Add("brnardica.bozic@skole.hr");
                        mm.BodyEncoding = UTF8Encoding.UTF8;
                        mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                        mm.IsBodyHtml = true;
                        client.Credentials = myCredential;
                        label1.Text = "Sending";
                        //IPHostEntry dnsInfo = Dns.Resolve("smtp.skole.hr");
                        try
                        {
                            //client.SendAsync()
                            client.Send(mm);
                            brmails += 1;
                            label1.Text = "list[0].ToString()";
                            foreach (var itemuid in list)
                            {
                                updateMail(itemuid);
                            }
                        }
                        catch (Exception ex)
                        {
                            label1.Text = ex.ToString();
                            return 1;
                        }
                      

                    }
                }
                    return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private int sendMailSolo(IEnumerable<ChangeManagementDetail> group, Guid gd)
        {
            try
            {
                using (dBTeslaEntities ctx = new dBTeslaEntities())
                {
                    foreach (var chdetail in group)
                    {
                        CMbyIDTch_Result res = new CMbyIDTch_Result();
 
                        IEnumerable<CMbyIDTch_Result> resp1 = ctx.Database.SqlQuery<CMbyIDTch_Result>(
                                "EXEC CMbyIDTch @ParentGuid,@ParentGuidTCH",
                                new SqlParameter("@ParentGuid", chdetail.ID_CMaster),
                                new SqlParameter("@ParentGuidTCH", gd)
                                ).ToList();
                        int i = resp1.Count();
                        //CMbyID_Result4.Parameters.Clear();
                        foreach (var chdetaildmail in resp1)
                        {
                            if (chdetaildmail.Mail == 0)
                            {
                                Guid uidtsch = new Guid(chdetaildmail.TeachersIDCM.ToString());
                                var sadresa = ctx.Teachers.SingleOrDefault(c => c.ID == chdetaildmail.ID_TeachersCM);
                                if (sadresa != null)
                                {
                                    if (emailIsValid(sadresa.Email))
                                    {
                                        int isend = sendMail1(chdetaildmail, sadresa.Email);
                                        if (isend == 1)
                                        {
                                            Guid uidCMdetail = new Guid(chdetaildmail.IDCMdetail.ToString());
                                            updateMail(uidCMdetail);
                                        }
                                    }
                                }
                               
                            }
                        }
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private int sendMail1(CMbyIDTch_Result resp, string smailCM)
        {
            try
            {
                String disc = " <p style='width:80%;margin-left:20px;'><b>IZJAVA O ODRICANJU OD ODGOVORNOSTI:</b> Sadržaj ove poruke i eventualno priloženih datoteka " +
                "povjerljiv je i namijenjen samo primateljima navedenima u adresi. Svako neovlašteno korištenje," +
                "objavljivanje, prerada, obrada, reprodukcija, prikazivanje, prenošenje, snimanje ili bilo koji " +
                "drugi oblik neovlaštene uporabe ove elektroničke pošte podliježe kaznenoj i prekršajnoj odgovornosti " +
                "kao i građansko pravnoj zaštiti.</p>" +
                "<p style='width:80%;margin-left:20px;'>Ako ste ovu poruku primili greškom, molimo Vas da o tome odmah obavijestite pošiljatelja i izbrišete " +
                "ovu poruku, njene privitke i kopije. Internet komunikacije su nesigurne i I.tehnička škole Tesla ne " +
                "preuzima nikakvu odgovornost s obzirom na bilo koju moguću netočnost bilo kojeg podatka koji je " +
                "sadržan u ovoj poruci, ako takav podatak nije povezan s registriranim predmetom rada I.tehničke škole Tesla.</p><br>";

                String poruka = "";


                if (resp.Poruka != null)
                {
                    String tekst = " <font color='#FF4500'>Dodatna obavijest:</font><br><br>" + resp.Poruka;
                    poruka = "<td style='width:60%;vertical-align:baseline;padding:15px 15px 15px 15px;background-color:#555555;color:#FAFAFA; -webkit-font-smoothing: antialiased;' rowspan='8'>" + tekst + "</td></tr> ";
                }
                DateTime date = Convert.ToDateTime(resp.Datum);
                String datum = date.ToString("dd.MM.yyyy");
                SmtpClient client = new SmtpClient();
                client.Port = 25;
                client.Host = "smtp.skole.hr";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                String postovani = "<p style='margin-left:10px;'>dodijeljena Vam je zamijena čije detalje možete vidjeti u nastavku.</p>";

                String satnicarka = "<p style='margin-left:20px;font-size: 10px;'>Za sve dodatne informacije možete kontaktirati satničarku u uredovno vrijeme</p>";
                satnicarka = satnicarka + "<p style='margin-left:20px;font-size: 10px;'>A/B turnus svaki dan od 13:30 - 15:00</p>";
                satnicarka = satnicarka + "<p style='margin-left:20px;font-size: 10px;'>B/A turnus svaki dan od 7:30 - 9:00</p>";
                satnicarka = satnicarka + "<p style='margin-left:20px;font-size: 10px;'>na tel 099 48 777 36.</p>";

                client.Credentials = new System.Net.NetworkCredential("tesla.info@skole.hr", "CBUN42ef");
                String Body = "<html><body style='background-color: #d9e6f2;margin-left:-10px;margin-right:0px; padding:0;'>" +
                    "<br><h2 style='margin-left:10px;margin-top:0px;'>I. tehnička škola Tesla</h2><hr>" +
                    "<p style='margin-left:10px;'>Poštovani.</p>" + postovani + "<br>" +
                    "<table style='border: 1px solid black;padding:10px;border-collapse: collapse;font-weight: bold;margin-left:20px;'> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Datum zamjene:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + datum + "</td>" +
                    poruka + "</tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Razred:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.Razred + "</td></tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Odsutni nastavnik:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.Name + "</td></tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Nastavnik u zamjeni:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.ProfCM + "</td></tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Predmet:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.SubjectsEx + " </td></tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Predmet u zamjeni:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.SubjectsCm + "</td></tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Kabinet:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.Kabinet + "</td></tr> " +
                    "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Sat:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + resp.Short + ". " + resp.Sat + "</td></tr> " +
                    "</table>" + satnicarka +
                   "<br><br><br><p style='margin-left:20px;font-size: 10px;'>" + disc + "</p></html></body>";
                String sSubject = "Zamjena: " + datum;//msdbozic@gmail.com  filip.matesa@gmail.com; zvonimir.petkovic@gmail.com; tomislavjurisic@net.hrmsdbozic@gmail.com

                MailMessage mm = new MailMessage("tesla.info@skole.hr", smailCM, sSubject, Body);
                //mm.Bcc.Add("brnardica.bozic@skole.hr");
                //mm.Bcc.Add("ivica.jelavic@skole.hr");
                // mm.Bcc.Add("ivcajelavic@gmail.com");
                //mm.Bcc.Add("zvonimir.petkovic@skole.com");
                //mm.Bcc.Add("tomislav.jurisic5@skole.hr");
                //mm.Bcc.Add("ijelavic4@net.amis.hr");
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                mm.IsBodyHtml = true;

                if (smailCM != null)
                {
                    if (emailIsValid(smailCM))
                    {
                        try
                        {
                            client.Send(mm);
                            brmails += 1;
                        }
                        catch (Exception ex)
                        {

                            label1.Text = ex.ToString();
                        }


                        // i=+1;
                        //label1.Text = i.ToString();
                    }

                }

                return 1;
            }
            catch (Exception)
            {
                return 1;
            }
        }
        private void updateMail(Guid uidCM)
        {
            label1.Text = uidCM.ToString();
            try
            {
                using (dBTeslaEntities ctx = new dBTeslaEntities())
                {
                    ChangeManagementDetail detail = ctx.ChangeManagementDetail
                                 .SingleOrDefault(c => c.ID == uidCM);


                        if (detail != null)
                        {
                            detail.Datum = DateTime.Now;
                            detail.Mail = 1;
                            ctx.SaveChanges();
                        }
                    

                    else
                    {
                        //MessageBox.Show("Već postoji najava izostanka.");
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void updateMailMaster(Guid uidCM)
        {
            try
            {
                using (dBTeslaEntities ctx = new dBTeslaEntities())
                {
                    ChangeManagementMaster detail = ctx.ChangeManagementMaster
                                 .SingleOrDefault(c => c.ID == uidCM);


                    if (detail != null)
                    {
                        detail.Mail = 2;
                        detail.Status = 2;
                        ctx.SaveChanges();
                    }


                    else
                    {
                        //MessageBox.Show("Već postoji najava izostanka.");
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {


            String poruka = "<font color='#FF4500'>Dodatna obavijest :</font><br><br>Prvu priliku na utakmici složio je Juventus. Higuain je u 3. minuti pucao s desetak metara, ali preko grede. Stara dama je preuzela inicijativu i dvadesetak minuta pritiskala na Dinamova vrata. U 16. minuti Hernanes je pucao s 20 metara, a lopta je prošla kraj Šemperove lijeve stative." +
                 "Prvu priliku na utakmici složio je Juventus. Higuain je u 3. minuti pucao s desetak metara, ali preko grede. Stara dama je preuzela inicijativu i dvadesetak minuta pritiskala na Dinamova vrata. U 16. minuti Hernanes je pucao s 20 metara, a lopta je prošla kraj Šemperove lijeve stative.";

            DateTime date = new DateTime();
            date = DateTime.Now;
            String sdate = date.ToShortDateString();

            String datum = date.ToString("dd.MM.yyyy");
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.Host = "smtp.skole.hr";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("tesla.info@skole.hr", "CBUN42ef");
            String Body = "<html><body style='background-color: #d9e6f2;margin-left:-10px;margin-right:0px; padding:0;'>" +
                "<br><h1 style='margin-left:30px;margin-top:0px;'>I. tehnička škola Tesla</h2><hr>" +
                "<br><p style='margin-left:10px;'>Poštovani.(to:" + sdate + ")</p><br><br><br><br>" +
                "<table style='width:90%;border: 1px solid black;padding:10px;border-collapse: collapse;font-weight: bold;margin-left:20px;'> " +
                "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Datum zamjene:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + datum + "</td>"+
                "<td style='width:60%;vertical-align:baseline;padding:15px 15px 15px 15px;background-color:#555555;color:#FAFAFA; -webkit-font-smoothing: antialiased;' rowspan='8'>" + poruka +"</td></tr> " +
                "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Razred:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>4R</td></tr> " +
                "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Odsutni nastavnik:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>Pero  Perić</td></tr> " +
                "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Nastavnik u zamjeni:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>Marko Marković</td></tr> " +
                "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Predmet:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>Oskjsdfljdsfj</td></tr> " +
                "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Predmet u zamjeni:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>Gashdfid ijasdhoiasdfh iudhfo</td></tr> " +
                "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Kabinet:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + "člklčklčklčklčk" + "</td ></tr> " +
                "<tr><td style='border: 1px solid black;text-align: right;padding:10px;background-color:#ccc;color:#336699;'>Sat:</td><td style='border: 1px solid #CDF5B3;padding:10px;padding-left: 10px;background-color:#19334d;color:#fff;'>" + "3" + ". " + "12.-13,55" + "</td></tr> " +
                "</table>" +
                "<br><br><br><p style='margin-left:10px;font-size: 15px;'>" + "resp.Poruka" + "</p></html></body>";
            String sSubject = "Zamjena: " + datum;//msdbozic@gmail.com
            MailMessage mm = new MailMessage("tesla.info@skole.hr", "ivica.jelavic@skole.hr", sSubject, Body);
            //mm.Bcc.Add("ijelavic4@net.amis.hr");
           // mm.Bcc.Add("ivcajelavic@gmail.com");
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
            mm.IsBodyHtml = true;
            client.Send(mm);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public static bool emailIsValid(string email)
        {
            string expresion;
            expresion = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
            if (Regex.IsMatch(email, expresion))
            {
                if (Regex.Replace(email, expresion, string.Empty).Length == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

  
    }
}






