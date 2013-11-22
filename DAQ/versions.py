import os
import sys


def filter(line):
	#print line
	# TODO: get new version
	if line.startswith("[") and line.find("AssemblyFileVersion(") > 0:
		return "[assembly: AssemblyFileVersion(\"1.0.0.5\")]\n"
	else:
		return line

def update_versions(assembly_info_file):
	file = open(assembly_info_file, 'r')
	contents = []
	if file:
		contents = file.readlines()
		file.close()

	lines = [filter(l) for l in contents]
	
	assembly_info_file2 = os.path.dirname(assembly_info_file) + "\\AssemblyInfo2.cs"
	file = open(assembly_info_file2, "w")
	if file:
		file.writelines(lines)
	file.close()
	os.remove(assembly_info_file)
	os.rename(assembly_info_file2, assembly_info_file)



def main(args):
	"""
	"""
	path = os.getcwd() + "\\..\\..\\%s\\Properties\\AssemblyInfo.cs" % args[0]
	print path
	if os.path.exists(path):
		update_versions(path)
	else:
		pass



if __name__ == "__main__":
    main(sys.argv[1:])

