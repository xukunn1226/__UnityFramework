#!/bin/bash
echo "hello world"

# 这是单行注释
# 伪装成无人调用的函数，用{}包含需要注释的多行
noexec() {
sfsdf32ewerew
dsfsd
ewe4
}
# 另一种多行注释方法
<< !
echo sdfs
echo "sdfs"
!

: '
echo 'ni'
echo "ni"
echo "ni"
echo "ni"
echo "ni"
echo "ni"
echo "ni"
echo "ni"
'

# 定义变量
your_name='xukun'
echo $your_name
echo ${your_name}

####################### 字符串
# 单引号：原样输出，不可有变量，不支持转义符
str='this is a string'

# 双引号：支持变量，转义符
your_name="qinjx" 
greeting="hello, \"$your_name\" !" 
greeting_1="hello, ${your_name} !" 
echo $greeting 
echo $greeting_1

# 字符串长度
str="abcde"
echo ${#str}

# 提取字符串
str="alibaba is a great company"
echo ${str:1:4}

# 查找字符串
string="alibaba is a great company"
echo `expr index "$string" is1`



####################### 条件判断
# 判断语句  https://www.cnblogs.com/fcing/p/9382418.html   https://blog.csdn.net/zhan570556752/article/details/80399154
[ 1 -eq 2 ]&&{
    echo aa;
    echo bb;
}||{
    echo cc
    echo dd
}

<<!
read -p "Input your path:" path
if [ -e $path -a -L $path ];then 
    echo "$0 is link file"
elif [ ! -e $path ] && [ -L $path ];then
    echo "$0 is not effctive link file"
elif [ -d $path ];then
    echo "$0 is a dirctory"
elif [ -f $path ];then
    echo " $0 is file"
else
    echo "$0 is other file"
fi
!



#!/bin/sh

SYSTEM=`uname -s`    #获取操作系统类型，我本地是linux
if [ $SYSTEM = "Linux" ];then
echo "Linux" 
elif [ $SYSTEM = "FreeBSD" ];then
echo "FreeBSD" 
elif [ $SYSTEM = "Solaris" ];then
echo "Solaris" 
else 
echo $SYSTEM
fi



# 文件类型判断
<<!
-e 是否存在 不管是文件还是目录，只要存在，条件就成立
-f 是否为普通文件
-d 是否为目录
-S socket
-p pipe
-c character
-b block
-L 软link
!

# 文件内容判断
<<!
-s 是否为非空文件
!-s 表示空文件
!

# 文件权限判断
<<!
-r  当前用户对其是否可读
-w 当前用户对其是否可写
-x 当前用户对其是否可执行
-u 是否有suid
-g 是否sgid
-k 是否有t位
!

# 两个文件的比较判断
<<!
file1 -nt file2	 比较file1是否比file2新
file1 -ot file2	 比较file1是否比file2旧
file1 -ef file2	 比较是否为同一个文件，或者用于判断硬连接，是否指向同一个inode
!

# 整数之间的判断
<<!
-eq	 相等
-ne	 不等
-gt	 大于
-lt  	 小于
-ge	 大于等于
-le	小于等于
!

# 字符串之间的判断
<<!
-z  是否为空字符串 字符串长度为0，就成立
-n  是否为非空字符串 只要字符串非空，就是成立
string1 = string2    是否相等           --等号两边要有空格
string1 != string2   不等
! 结果取反
!




# IF ELSE
string='My long string'
if [[ $string == *"My long"* ]]; then
  echo "It's there!"
fi


string='My long string'
if [[ $string != "${string/foo/}" ]]; then
    echo "It's there!"
else
	echo "It's not there!"
fi

# swtich case
<<!
echo '输入 1 到 4 之间的数字:'
echo '你输入的数字为:'
read aNum
case $aNum in
    1)  echo '你选择了 1'
    ;;
    2)  echo '你选择了 2'
    ;;
    3|4)  echo '你选择了 3 或 4'
    ;;
    *)  echo '你没有输入 1 到 4 之间的数字'
    ;;
esac
!

# for
for((i=1;i<=10;i++));  
do   
echo $(expr $i \* 3 + 1);  
done

for i in {1..10}  
do  
echo $(expr $i \* 3 + 1);  
done  

for i in `ls`;  
do   
echo $i is file name\! ;  
done   

for i in f1 f2 f3 ;  
do  
echo $i is appoint ;  
done  

for file in /proc/*;  
do  
echo $file is file path \! ;  
done  

for file in $(ls *.sh)  
do  
echo $file is file path \! ;  
done  

# do while
<<!
while getopts ":a:b:c:" opt
do
	case $opt in
		a)
		echo "参数a的值$OPTARG"
		;;
		b)
		echo "参数b的值$OPTARG"
		;;
		c)
		echo "参数c的值$OPTARG"
		;;
		?)
		echo "未知参数"
		exit 1
		;;
	esac
done
!



# 函数
function pause()
{
	read -n 1 -p "$*" INP
    if [ $INP != '' ] ; then
		echo -ne '\b \n'
    fi
}
pause 'Press any key to continue...'














for skill in Ada Coffe Action Java; do 
    echo "I am good at ${skill}Script" 
done



