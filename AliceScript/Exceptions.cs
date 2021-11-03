﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AliceScript
{
   public enum Exceptions
    {
        /// <summary>
        /// 既定のエラーコード
        /// </summary>
        NONE=0x000,
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
        PROPERTY_OR_METHOD_NOT_FOUND=0x00c,
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
        INSUFFICIENT_ARGUMETS = 0x014,
        /// <summary>
        /// パッケージファイルが壊れています
        /// </summary>
        BAD_PACKAGE = 0x015,
        /// <summary>
        /// 継承元クラスが存在しません
        /// </summary>
        NOT_EXISTS_INHERITANCE_BASE = 0x016,
        /// <summary>
        /// 配列に要素がありません
        /// </summary>
        ARRAY_IS_DOESNT_HAVE_TUPLE = 0x017,
        /// <summary>
        /// 変数がNullです
        /// </summary>
        VARIABLE_IS_NULL = 0x018,
        /// <summary>
        /// ユーザー定義の例外です
        /// </summary>
        USER_DEFINED = 0x019,
        /// <summary>
        /// parmsキーワードより後にパラメータを追加することはできません
        /// </summary>
        COULDNT_ADD_PARAMETERS_AFTER_PARMS_KEYWORD=0x01a,
        /// <summary>
        /// ファイルが見つかりません
        /// </summary>
        FILE_NOT_FOUND=0x01b,
        /// <summary>
        /// 名前空間が存在しません
        /// </summary>
        NAMESPACE_NOT_FOUND=0x01c,
        /// <summary>
        /// 名前空間が読み込まれていません
        /// </summary>
        NAMESPACE_NOT_LOADED=0x01d,
        /// <summary>
        /// 繰り返しが多すぎます
        /// </summary>
        TOO_MANY_REPETITIONS=0x01e,
        /// <summary>
        /// Catchステートメントがありません
        /// </summary>
        MISSING_CATCH_STATEMENT=0x01f,
        /// <summary>
        /// ブロックを実行できませんでした
        /// </summary>
        COULDNT_EXECUTE_BLOCK=0x020,
        /// <summary>
        /// 括弧が必要です
        /// </summary>
        NEED_BRACKETS=0x021,
        /// <summary>
        /// 括弧は不要です
        /// </summary>
        UNNEED_TO_BRACKETS=0x022,
        /// <summary>
        /// その型の変数は使用できません
        /// </summary>
        WRONG_TYPE_VARIABLE=0x023
    }
}
