

import MySQLdb

conn=MySQLdb.connect(host='127.0.0.1',user='root', passwd='root', db='scada', port=3306)

try:
	for i in xrange(1, 1000):
	    
	    cur=conn.cursor()
	    c = cur.execute('select * from CinderellaData_Rec')
	    #print c
	    cur.close()
	    
except MySQLdb.Error,e:
     print "Mysql Error %d: %s" % (e.args[0], e.args[1])

conn.close()