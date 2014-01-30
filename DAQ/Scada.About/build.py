
from xml.dom.minidom import parse
import os
import codecs
import ast
import shutil

path = os.path.dirname(os.path.abspath(__file__)) + "\\Resources\\Features.XML"

dom = parse(path)
if (dom):
	shutil.copyfile(path, path + ".bank")

print "Pre-build Action"
node = dom.getElementsByTagName("features")[0]

build = node.getAttribute("build")
node.setAttribute('build', str(ast.literal_eval(build) + 1))

f = file(path, 'w')
writer = codecs.lookup('utf-8')[3](f)
dom.writexml(writer, encoding='utf-8')
f.close()