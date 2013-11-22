
import os
import sys
import shutil
import zipfile 



def add_zipfile(zf, filename, root):
    path = os.path.relpath(filename, root)
    print path
    zf.write(filename, path)

otherfilelist = ["agent.settings", "local.ip", "password", "scada.sql", "start_vdata.bat", "stop_vdata.bat", "startup.bat"]

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




def main(args):
    path = os.getcwd() + "\\" + args[0]

    type = ""
    if len(args) > 1:
        type = args[1]
    make_bin_zip(path, os.getcwd(), type)

if __name__ == "__main__":
    main(sys.argv[1:])