using System;
using System.Collections.Generic;
using System.Text;

namespace AliceScript
{
    enum Exceptions
    {
        /// <summary>
        /// 関数が見つかりません
        /// </summary>
        COULDNT_FIND_FUNCTION=0x001,
        /// <summary>
        /// 配列が見つかりません
        /// </summary>
        COULDNT_FIND_ARRAY=0x002,
        /// <summary>
        /// 演算子が見つかりません
        /// </summary>
        COULDNT_FIND_OPERAND=0x003,
        /// <summary>
        /// 変数が見つかりません
        /// </summary>
        COULDNT_FIND_VARIABLE=0x004,
        /// <summary>
        /// 指定された名前は予約されています
        /// </summary>
        ITIS_RESERVED_NAME=0x005,
        /// <summary>
        /// 先頭の文字に数字または'-'を使用することはできません
        /// </summary>
        ITHAS_ILLEGAL_FIRST_CHARACTER=0x006,
        /// <summary>
        /// 変数名に不正な文字が含まれています
        /// </summary>
        VARIABLE_NAME_CONTAINS_ILLEGAL_CHARACTER=0x007,
        /// <summary>
        /// 指定された変数名は使用できません
        /// </summary>
        ILLEGAL_VARIABLE_NAME=0x008,
        /// <summary>
        /// 引数が不完全です
        /// </summary>
        INCOMPLETE_ARGUMENTS=0x009,
        /// <summary>
        /// 関数の定義が不完全です
        /// </summary>
        INCOMPLETE_FUNCTION_DEFINITION=0x00a,
        /// <summary>
        /// そのようなオブジェクトは存在しません
        /// </summary>
        OBJECT_DOESNT_EXIST=0x00b,
        /// <summary>
        /// 変数または関数が存在しません
        /// </summary>
        VARIABLE_OF_FUNCTION_DOESNT_EXIST=0x00c,
        /// <summary>
        /// 引数が不完全です
        /// </summary>
        INVAILD_ARGUMENT=0x00d,
        /// <summary>
        /// 関数内の引数が不完全です
        /// </summary>
        INVAILD_ARGUMENT_FUNCTION = 0x00e,
        /// <summary>
        /// 配列が必要です
        /// </summary>
        EXPECTED_ARRRAY=0x00f,
        /// <summary>
        /// 数値型である必要があります
        /// </summary>
        EXPECTED_NUMBER=0x010,
        /// <summary>
        /// 整数型である必要があります
        /// </summary>
        EXPECTED_INTEGER=0x011,
        /// <summary>
        /// 負でない整数である必要があります
        /// </summary>
        EXPECTED_NON_NEGATIVE_INTEGER=0x012,
        /// <summary>
        /// 自然数である必要があります
        /// </summary>
        EXPECTED_NATURAL_NUMBER=0x013,
        /// <summary>
        /// 引数が不足しています
        /// </summary>
        INSUFFICIENT_ARGUMETS = 0x014
    }
}
