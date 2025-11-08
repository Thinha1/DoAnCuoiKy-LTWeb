# Quy trình làm việc với github

## Khi bắt đầu làm việc
1. Cập nhật repository mới nhất
```bash
git pull origin main
```
2. Tạo nhánh riêng để làm việc
```bash
git checkout -b ten-nhanh-cua-ban
```
## Trong quá trình làm việc
1. Thêm file đã chỉnh sửa vào staging
```bash
git add .
```
2. Commit thay đổi với mô tả cụ thể
```bash
git commit -m "Mô tả ngắn về thay đổi"
```
3. Đẩy nhánh lên repository
```bash
git push origin ten-nhanh-cua-ban
```
4. Tạo Pull Request (PR) trên GitHub để review và merge vào nhánh chính

## Khi kết thúc công việc
1. Chuyển về nhánh chính
```bash
git checkout main
```
2. Cập nhật lại repository mới nhất
```bash
git pull origin main
```
