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
        # _offset = offset
        while read_byte(offset):
            text += read_bytes(offset, 1).decode()
            offset += 1
        # print(f"0x{_offset:08X}: self.tr('{text}'),")
        return text


# if __name__ == '__main__':
#     from dolphin_memory_engine import hook
#
#     hook()
#     for i in range(32):
#         t = Text.get_text(0x8080A614 + 0x18 * i)
        # print(f"'{t}': self.tr('{t}'),")
