

import os

ps = ["hpic", "weather", "shelter", "dwd", "hpge", "labr", "hvsampler", "isampler", "cinderella.data", "cinderella.status"]

for p in ps:
	try:
		dest = "scada.%s" % p
		os.rename("!" + dest, dest)
	except:
		pass
