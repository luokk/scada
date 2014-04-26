#!/usr/bin/env python
# -*- coding: utf-8 -*-

import urllib2

# Data Post for HPIC
data = """
{
  "station": "128",
  "token": "",
  "entry": [{
      "device": "hpic",
      "time": "2014/4/24 23:29:00",
      "0102060301": 99.0,
      "0102069901": 123.0,
      "0102069902": 404.0,
      "0102069903": 24.0
    } ] }
"""

