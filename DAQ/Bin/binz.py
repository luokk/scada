#!/usr/bin/env python
# -*- coding: utf-8 -*-

import os
import sys
import shutil
import zipfile 



def add_zipfile(zf, filename, root):
    path = os.path.relpath(filename, root)
    print path
    zf.write(filename, path)

otherfilelist = ["agent.settings", "mainvs.settings", "local.ip", "password", "scada.sql", "as1.type", "start_vdata.bat", "stop_vdata.bat", "startup.bat"]

def make_bin_zip(filepath, root, type):
    # root = filepath
    zf = zipfile.ZipFile('bin.zip', 'w', zipfile.ZIP_DEFLATED)

    for t in [os.walk(filepath), os.walk(os.path.join(root, 'config'))]:
        for i in t:
            for f in i[2]:
                dirpath = i[0]
                filename = os.path.join(dirpath, f)
                if filename.endswith(".vshost.exe") or filename.endswith(".vshost.exe.manifest"):
                    continue

                if filename.endswith(".dll") or filename.endswith(".exe"):
                    add_zipfile(zf, filename, root)
                if type == "debug-info" and filename.endswith(".pdb"):
                    add_zipfile(zf, filename, root)
                if filename.endswith(".cfg"):
                    add_zipfile(zf, filename, root)

                if f in otherfilelist:
                    add_zipfile(zf, filename, root)
    zf.close()          

def copy_formproxy(folder, curpath):
    print "!!!" + folder
    destpath = os.path.join(curpath, folder, "Scada.FormProxy.exe")
    srcfile = os.path.join(curpath, "Scada.FormProxy.ex")
    print srcfile, "****\n"
    print destpath, "****\n"
    shutil.copy(srcfile, destpath)

def main(args):
    path = os.getcwd() + "\\" + args[0]
    print args
    curpath = os.getcwd()
    type = ""
    if len(args) > 0:
        type = args[0]
    copy_formproxy(type, curpath)
    make_bin_zip(path, curpath, type)

if __name__ == "__main__":
    main(sys.argv[1:])