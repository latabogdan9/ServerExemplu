using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ServerExemplu
{
    class ClientHandler
    {
        private Socket _sk = null;
        private int _idx = -1;
        private Thread _th = null;
        private bool _shouldRun = true;
        private bool _isRunning = true;
        public ClientHandler(Socket sk, int id)
        {
            _sk = sk;
            _idx = id;
        }

        public void initClient()
        {
            if (null != _th)
                return;

            _th = new Thread(new ThreadStart(run));
            _th.Start();
        }

        public void stopClient()
        {
            if (_th == null )
                return;

            _sk.Close();
            _shouldRun = false;
        }

        public bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(10000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        private async Task handleMsgAsync(String msg)
        {
            //mesaj-email-parola-nume-app                     
            //prelucrare msg.....
            Console.WriteLine(msg);
            char[] sep = {'-'};
            String[] arrMsg = msg.Split(sep);
            Console.WriteLine(arrMsg[0]);
            Console.WriteLine(arrMsg[1]);
            Console.WriteLine(arrMsg[2]);
            Console.WriteLine(arrMsg[3]);
            //Console.WriteLine(arrMsg[4]);
            if (arrMsg[0].StartsWith(Mesaje.sLoginReq) && arrMsg[3].StartsWith("generic"))
            {
                Utilizator res = await Task.Run(() => CheckIfUser(arrMsg[1], arrMsg[2],arrMsg[3]));
                if (res==null)
                {
                    Console.WriteLine("Nu exista cont");
                    sendResponseLogin(Mesaje.sLoginErr);
                }
                else {
                    Console.WriteLine("Am gasit cont");
                    sendResponseLogin(Mesaje.sLoginOK); }

            }

            else if (arrMsg[0].StartsWith(Mesaje.sLoginReq) && arrMsg[3].StartsWith("auth"))
            {
                Utilizator res = await Task.Run(() => CheckIfUser(arrMsg[1], arrMsg[2], arrMsg[3]));
                if (res==null)
                {
                    sendResponseLogin(Mesaje.sLoginErr);
                }
                else sendResponseLogin(Mesaje.sLoginOK);

            }
            else if (arrMsg[0].StartsWith(Mesaje.sRegisterReq) && arrMsg[4].StartsWith("generic"))
            {
                Utilizator res = await Task.Run(() => CheckIfUser(arrMsg[1], arrMsg[2], arrMsg[4]));
                if (res!=null)
                {
                    Console.WriteLine("contul exista");
                    sendResponseLogin(Mesaje.sLoginErr);
                }
                else
                {
                    Console.WriteLine("adaug cont");
                    using (UserDbContext dbContext = new UserDbContext())
                    {
                        Utilizator u = new Utilizator();
                        u.nume = arrMsg[3];
                        u.email = arrMsg[1];
                        u.password = arrMsg[2];
                        u.app = arrMsg[4];

                        dbContext.Useri.Add(u);
                        dbContext.SaveChanges();
                    }
                    sendResponseLogin(Mesaje.sLoginOK);
                }
            }

            else if (arrMsg[0].StartsWith(Mesaje.sRegisterReq) && arrMsg[4].StartsWith("auth"))
            {
                Utilizator res = await Task.Run(() => CheckIfUser(arrMsg[1], arrMsg[2], arrMsg[4]));
                if (res!=null)
                {
                    sendResponseLogin(Mesaje.sLoginErr);
                }
                else
                {
                    using (UserDbContext dbContext = new UserDbContext())
                    {
                        Utilizator u = new Utilizator();
                        u.nume = arrMsg[3];
                        u.email = arrMsg[1];
                        u.password = arrMsg[2];
                        u.app = arrMsg[4];

                        dbContext.Useri.Add(u);
                        dbContext.SaveChanges();
                    }
                    sendResponseLogin(Mesaje.sLoginOK);
                }
            }

        }

        public Utilizator CheckIfUser(string user, string pass,string app)
        {

            using (UserDbContext dbContext = new UserDbContext())
            {
                try
                {
                    Utilizator u = dbContext.Useri.SingleOrDefault(us => us.email == user && us.password==pass && us.app==app);
                    if (u == null)
                    {
                        return null;
                    }
                    else
                    {
                        return u;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }

            }

        }
        private void sendResponseLogin(String msg)
        {
            //procesare si trimitere raspuns in cazul mesajului Login
            //verificam daca exista user-ul...
            String raspuns = "";
            //if user exists?
            raspuns = raspuns.Insert(0, "login");
            raspuns = raspuns.Insert(4, "ok");
            byte[] bytesMsgRaspuns = Encoding.ASCII.GetBytes(msg);
            _sk.Send(bytesMsgRaspuns);
        }
        
        private void run()
        {
            // Attention! This is the largest message one can receive!
            

            while (_shouldRun)
            {
                //Console.WriteLine("Client... "+_idx);
                byte[] rawMsg = new byte[50];
                try
                {
                    
                        int bCount = _sk.Receive(rawMsg);
                        String msg = Encoding.UTF8.GetString(rawMsg);
                        if (bCount > 0)
                            Console.WriteLine("Client " + _idx + ": " + msg);
                        handleMsgAsync(msg);
                                     
                        
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Client exxxccp ");
                    return;
                }
                Thread.Sleep(1);
            }
            _isRunning = false;
            
        }

        public bool isAlive()
        {
            return _isRunning;
        }
    }
}
