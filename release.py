#!/usr/bin/env python
# -*- coding: utf-8 -*-

import os
import py_compile
import shutil
import sys


def remove_cache(folder: str):
    for r, dirs, _ in os.walk(folder):
        [shutil.rmtree(os.path.join(r, d)) for d in dirs if d == '__pycache__']


def release_project(s_path):
    cache = '.cpython-' + sys.version[0:4].replace('.', '') + '.pyc'
    count = 0
    p_folder = os.path.split(s_path)[0]
    p_name = p_folder.split('/')[-1]

    remove_cache(p_folder)

    r_folder = p_folder.rstrip(p_name) + 'release'
    if os.path.exists(r_folder):
        shutil.rmtree(r_folder)
    os.mkdir(r_folder)

    for r, _, files in os.walk(p_folder):
        for f in files:
            f_path = os.path.join(r, f)
            if os.path.normcase(f_path) == os.path.normcase(s_path) or not f_path.endswith('.py'):
                shutil.copy(f_path, f_path.replace(p_name, 'release'))
            else:
                c_path = os.path.join(r, '__pycache__', f.replace('.py', cache))
                t_path = f_path.replace(p_name, 'release') + 'c'
                py_compile.compile(f_path)
                if os.path.exists(c_path):
                    if not os.path.exists(t_root := os.path.dirname(t_path)):
                        os.mkdir(t_root)
                    shutil.move(c_path, t_path)
                    count += 1

    remove_cache(p_folder)

    print(f'0 INFO: release completed. convert {count} files.')

release_project(sys.argv[1])
