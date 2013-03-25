#!/usr/bin/env python
#Boa:App:BoaApp

import wx

import tvf

modules ={u'tvf': [1, 'Main frame of Application', u'tvf.py']}

class BoaApp(wx.App):
    def OnInit(self):
        self.main = tvf.create(None)
        self.main.Show()
        self.SetTopWindow(self.main)
        return True

def main():
    application = BoaApp(0)
    application.MainLoop()

if __name__ == '__main__':
    main()
