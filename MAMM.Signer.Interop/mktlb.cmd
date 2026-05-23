@if not %1=="" (
    echo Running DevCmd...
    call %1 1>nul
)

echo Running MIDL...
midl %2 /tlb %3
