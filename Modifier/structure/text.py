#!/usr/bin/env python
# -*- coding: utf-8 -*-

from dolphin_memory_engine import read_byte, read_word, read_bytes


class Text:
    @classmethod
    def get_text(cls, offset: int) -> str:
        temp = read_word(offset)
        if (temp > 0x80000000) and (temp < 0x81800000):
            return cls.get_text(temp)
        text = ''
        while read_byte(offset):
            text += read_bytes(offset, 1).decode()
            offset += 1
        return text
