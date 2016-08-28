﻿using CodeNote.domain.tomato;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace CodeNote
{
    public partial class Tomato_work : Form
    {
        //TODO: 对tomato_user细项记录
        //TODO: TASK增加勾选完成选项
        //TODO: 完整TASK右键菜单
        //TODO: 实现到点时间通知
        //TODO: 实现窗口置顶/取消置顶

        //窗体
        private static Tomato_setting tomato_setting;
        //运行时变量
        private string time_state = "init";
        private DateTime time = new DateTime();
        private DateTime timenow = new DateTime();
        private TimeSpan tmspan=new TimeSpan();
        private int tomato_now_cylce = 1;
        private int tomato_count = 0;
        //cfg参数
        private int cfg_tomato_tm = 1500;
        private int cfg_break_tm = 300;
        private int cfg_long_break_tm = 900;
        private int cfg_tomato_cylce = 4;
        //list参数
        private ArrayList tasklist = new ArrayList(); //tomato_task

        public Tomato_work()
        {
            InitializeComponent();
        }

        private void Tomato_work_Load(object sender, EventArgs e)
        {
            time_label.Text = "00:00";
            Init();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timenow = DateTime.Now;
            if (time_state == "running_tm")
            {
                tmspan = time - timenow;
                if(tmspan.Ticks>0){
                    time_label.Text = tmspan.Minutes + ":" + tmspan.Seconds;
                }else {
                    timenow = DateTime.Now;
                    if (tomato_now_cylce < cfg_tomato_cylce)
                    {
                        time = DateTime.Now.AddSeconds(cfg_break_tm);
                        now_state_lb.Text = "休息时间";
                    }
                    else
                    {
                        time = DateTime.Now.AddSeconds(cfg_long_break_tm);
                        now_state_lb.Text = "长休息时间";
                    }
                    tomato_count++;
                    if (GetXmlConfig("config/tomato_user.xml", "/user/tomato_today_date") != DateTime.Now.ToString("yyyy-MM-dd"))
                    {
                        SetXmlConfig("config/tomato_user.xml", "/user/tomato_today", "0");
                        SetXmlConfig("config/tomato_user.xml", "/user/tomato_today_date", DateTime.Now.ToString("yyyy-MM-dd"));
                    }
                    SetXmlConfig("config/tomato_user.xml", "/user/tomato_today", Convert.ToString(Convert.ToInt32(GetXmlConfig("config/tomato_user.xml", "/user/tomato_today")) + 1));
                    SetXmlConfig("config/tomato_user.xml", "/user/tomato_count", Convert.ToString(Convert.ToInt32(GetXmlConfig("config/tomato_user.xml", "/user/tomato_count")) + 1));
                    total_tomato_cnt_lb.Text = "番茄数:" + tomato_count;
                    today_tomato_cnt_lb.Text = "今日:" + GetXmlConfig("config/tomato_user.xml", "/user/tomato_today");
                    time_state = "running_break";
                }
            }
            else if (time_state == "running_break")
            {
                tmspan = time - timenow;
                if (tmspan.Ticks > 0)
                {
                    time_label.Text = tmspan.Minutes + ":" + tmspan.Seconds;
                }
                else
                {
                    if(tomato_now_cylce<cfg_tomato_cylce){
                        tomato_now_cylce++;
                    }else{
                        tomato_now_cylce = 1 ;
                        SetXmlConfig("config/tomato_user.xml", "/user/total_cycle", Convert.ToString(Convert.ToInt32(GetXmlConfig("config/tomato_user.xml", "/user/total_cycle") + 1)));
                        total_cycle_lb.Text = "总循环:" + GetXmlConfig("config/tomato_user.xml", "/user/total_cycle");
                    }
                    cycle_count_lb.Text = "循环:" + tomato_now_cylce + "/" + cfg_tomato_cylce;
                    timenow = DateTime.Now;
                    time = DateTime.Now.AddSeconds(cfg_tomato_tm);
                    time_state = "running_tm";
                    now_state_lb.Text = "工作时间";
                }
            }
        }
        /*
         * 开始 
         */
        private void toolStripLabel1_Click(object sender, EventArgs e)
        {
            if (time_state == "pause")
            {
                timenow = DateTime.Now ;
                time = DateTime.Now.AddSeconds(tmspan.TotalSeconds);
            }
            else if (time_state == "init")
            {
                timenow = DateTime.Now;
                time = DateTime.Now.AddSeconds(cfg_tomato_tm);
            }
            time_state = "running_tm";
            now_state_lb.Text = "工作时间";
            timer1.Enabled = true;
        }
        /*
         * 暂停 
         */
        private void toolStripLabel2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            time_state = "pause";
            now_state_lb.Text = "暂停";
        }
        /*
         * 重新开始 
         */
        private void toolStripLabel3_Click(object sender, EventArgs e)
        {
            timenow = DateTime.Now;
            time = DateTime.Now.AddSeconds(cfg_tomato_tm);
            time_state = "running_tm";
            now_state_lb.Text = "工作时间";
            timer1.Enabled = true;
        }
        /*
         * 重新开始循环 
         */
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            timenow = DateTime.Now;
            time = DateTime.Now.AddSeconds(cfg_tomato_tm);
            time_state = "running_tm";
            now_state_lb.Text = "工作时间";
            tomato_now_cylce = 1;
            cycle_count_lb.Text = "循环:" + tomato_now_cylce + "/" + cfg_tomato_cylce;
            timer1.Enabled = true;
        }
        /*
         * 设置 
         */
        private void toolStripLabel4_Click(object sender, EventArgs e)
        {
            if (tomato_setting == null || tomato_setting.IsDisposed)
            {
                tomato_setting = new Tomato_setting(this);
                tomato_setting.Show();
            }
        }
        /*
         * 新建任务按钮 
         */
        private void btn_AddTask_Click(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("config/tomato_list.xml");
            XmlNode listnode=doc.SelectSingleNode("/list");
            XmlNode lastnode = doc.SelectSingleNode("/list").LastChild;
            TomatoTask task = new TomatoTask();
            task.Id = Convert.ToInt32(lastnode.SelectSingleNode("id").InnerText.Trim()) + 1;
            task.Title = txt_title.Text;
            task.Content = txt_content.Text;
            task.Datetime = txt_time.Text;
            task.State = "0";
            XmlElement node_task = doc.CreateElement("task");
            XmlElement node_id = doc.CreateElement("id");
            node_id.InnerText = Convert.ToString(task.Id);
            XmlElement node_title = doc.CreateElement("title");
            node_title.InnerText = task.Title;
            XmlElement node_content = doc.CreateElement("content");
            node_content.InnerText = task.Content;
            XmlElement node_datetime = doc.CreateElement("datetime");
            node_datetime.InnerText = task.Datetime;
            XmlElement node_state = doc.CreateElement("state");
            node_state.InnerText = task.State;
            node_task.AppendChild(node_id);
            node_task.AppendChild(node_title);
            node_task.AppendChild(node_content);
            node_task.AppendChild(node_datetime);
            node_task.AppendChild(node_state);
            listnode.AppendChild(node_task);
            tasklist.Add(task);
            doc.Save("config/tomato_list.xml");
            txt_title.Text = "";
            txt_content.Text="";
            txt_time.Text="";
            task_list.DataSource = null;
            task_list.DataSource = tasklist;
        }
        public void Init()
        {
            //初始化cfg参数
            cfg_tomato_tm = Convert.ToInt32(GetXmlConfig("config/tomato_cfg.xml", "/config/tomato_tm"));
            cfg_break_tm = Convert.ToInt32(GetXmlConfig("config/tomato_cfg.xml", "/config/break_tm"));
            cfg_long_break_tm = Convert.ToInt32(GetXmlConfig("config/tomato_cfg.xml", "/config/long_break_tm"));
            cfg_tomato_cylce = Convert.ToInt32(GetXmlConfig("config/tomato_cfg.xml", "/config/tomato_cylce"));
            //初始化界面
            cycle_count_lb.Text = "循环:" + tomato_now_cylce + "/" + cfg_tomato_cylce;
            tomato_count = Convert.ToInt32(GetXmlConfig("config/tomato_user.xml", "/user/tomato_count"));
            today_tomato_cnt_lb.Text = "今日:" + GetXmlConfig("config/tomato_user.xml", "/user/tomato_today");
            total_tomato_cnt_lb.Text = "番茄数:" + tomato_count;
            total_cycle_lb.Text = "总循环:" + GetXmlConfig("config/tomato_user.xml", "/user/total_cycle");
            //初始化任务列表
            tasklist = GetXmlConfigList("config/tomato_list.xml", "/list/task");
            task_list.DataSource = tasklist;
        }
        private ArrayList GetXmlConfigList(string filename,string xpath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNodeList nodes=doc.SelectNodes(xpath);
            ArrayList list = new ArrayList();
            foreach (XmlNode node in nodes)
            {
                TomatoTask task=new TomatoTask();
                task.Id = Convert.ToInt32(node.SelectSingleNode("id").InnerText.Trim());
                task.Title = node.SelectSingleNode("title").InnerText.Trim();
                task.Content = node.SelectSingleNode("content").InnerText.Trim();
                task.Datetime = node.SelectSingleNode("datetime").InnerText.Trim();
                task.State = node.SelectSingleNode("state").InnerText.Trim();
                list.Add(task);
            }
            return list;
        }
        private string GetXmlConfig(string filename, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            return doc.SelectSingleNode(xpath).InnerText.Trim();
        }
        private void SetXmlConfig(string filename, string xpath, string value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            doc.SelectSingleNode(xpath).InnerText = value.Trim();
            doc.Save(filename);
        }
        private void DelXmlConfig(string filename, string parent_xpath,string childnodename,string value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNodeList nodes = doc.SelectNodes(parent_xpath);
            foreach (XmlNode node in nodes)
            {
                if (node.SelectSingleNode(childnodename).InnerText.Trim()==value)
                {
                    node.ParentNode.RemoveChild(node);
                    break;
                }
            }
            doc.Save(filename);
        }

        /*
         * 右键菜单
         */ 
        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TomatoTask selectedTask=(TomatoTask)task_list.SelectedValue;
            int id=selectedTask.Id;
            
            DelXmlConfig("config/tomato_list.xml", "/list/task","id",Convert.ToString(id));
            tasklist.Remove(task_list.SelectedValue);
            task_list.DataSource = null;
            task_list.DataSource = tasklist;
        }

        

    }
}